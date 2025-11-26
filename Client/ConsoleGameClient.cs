using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Client;
using SemestrovkaSockets;
using Server;

public class ConsoleGameClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private int _playerId = -1;
        private int _currentPlayerId = -1;
        private string _playerName = "";
        private List<Card> _hand = new List<Card>();

        public async Task StartAsync(string host, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();

            Console.WriteLine("Подключён к серверу. Введите имя:");
            _playerName = Console.ReadLine() ?? "Player";

            // Отправляем Join
            var joinPayload = Encoding.UTF8.GetBytes(_playerName);
            var packet = new byte[1 + joinPayload.Length];
            packet[0] = (byte)GameCommand.Join;
            Buffer.BlockCopy(joinPayload, 0, packet, 1, joinPayload.Length);

            await _stream.WriteAsync(packet, 0, packet.Length);

            // Слушаем ответы от сервера
            _ = Task.Run(ReadLoop);

            // Цикл ввода команд
            await InputLoop();
        }

        private async Task ReadLoop()
        {
            var buffer = new byte[1024];
            while (true)
            {
                try
                {
                    var size = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (size == 0) break;

                    var command = (GameCommand)buffer[0];
                    var payload = size > 1 ? buffer[1..size] : null;

                    switch (command)
                    {
                        case GameCommand.GameState:
                            HandleGameState(payload);
                            break;
                        case GameCommand.GameOver:
                            HandleGameOver(payload);
                            break;
                        case GameCommand.Error:
                            HandleError(payload);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка чтения: {ex.Message}");
                    break;
                }
            }
        }

        private void HandleGameState(byte[]? payload)
        {
            if (payload == null) return;

            var json = Encoding.UTF8.GetString(payload);
            var state = JsonSerializer.Deserialize<GameStateData>(json);

            // Обновляем ID текущего игрока
            _currentPlayerId = state.CurrentPlayerId;

            // Находим себя в списке игроков и обновляем свой ID
            var myPlayer = state.Players.FirstOrDefault(p => p.Name == _playerName);
            if (myPlayer != null && _playerId == -1)
            {
                _playerId = myPlayer.Id;
            }

            // Обновляем свои карты
            _hand.Clear();
            _hand.AddRange(state.YourHand.Select(c => new Card { Type = c.Type }));

            Console.Clear();
            Console.WriteLine("=== ТЕКУЩЕЕ СОСТОЯНИЕ ИГРЫ ===");

            Console.WriteLine("Игроки:");
            foreach (var p in state.Players)
            {
                var marker = p.Id == _playerId ? " [ВЫ]" : "";
                Console.WriteLine($"  {p.Name} (ID: {p.Id}) - {p.CardsCount} карт, жив: {p.IsAlive}{marker}");
            }

            Console.WriteLine($"\nСейчас ходит: {state.CurrentPlayerName} (ID: {state.CurrentPlayerId})");

            Console.WriteLine("\nХоды:");
            foreach (var m in state.Moves)
            {
                if (m.PlayerId == -1)
                    Console.WriteLine($"  [СТОЛ] Выложено: {m.DeclaredCount} x {m.DeclaredNominal}");
                else
                    Console.WriteLine($"  Игрок {m.PlayerId} выложил {m.DeclaredCount} x {m.DeclaredNominal}");
            }

            Console.WriteLine($"\nВаши карты: {string.Join(", ", _hand.Select(c => c.Type))}");
            Console.WriteLine("\nВведите команду: play, accuse, leave");
        }

        private void HandleGameOver(byte[]? payload)
        {
            if (payload == null) return;

            var json = Encoding.UTF8.GetString(payload);
            var result = JsonSerializer.Deserialize<GameOverData>(json);

            Console.WriteLine($"\n=== ИГРА ОКОНЧЕНА ===");
            Console.WriteLine($"Победитель: {result.WinnerName} (ID: {result.WinnerId})");
            Environment.Exit(0);
        }

        private void HandleError(byte[]? payload)
        {
            if (payload == null) return;
            var code = payload[0];
            Console.WriteLine($"Ошибка: {code}");
        }

        private async Task InputLoop()
        {
            string? input;
            while ((input = Console.ReadLine()) != null)
            {
                switch (input.ToLower())
                {
                    case "play":
                        await HandlePlay();
                        break;
                    case "accuse":
                        await HandleAccuse();
                        break;
                    case "leave":
                        await HandleLeave();
                        return;
                }
            }
        }

        private async Task HandlePlay()
        {
            // Если _playerId ещё не установлен, не проверяем
            if (_playerId != -1 && _currentPlayerId != _playerId)
            {
                Console.WriteLine("Сейчас не ваш ход.");
                return;
            }

            if (_hand.Count == 0)
            {
                Console.WriteLine("У вас нет карт для хода.");
                return;
            }

            Console.WriteLine($"Ваши карты: {string.Join(", ", _hand.Select((c, i) => $"{i} - {c.Type}"))}");
            Console.Write("Введите индексы карт, которые хотите выложить (через запятую): ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return;

            var indices = input.Split(',')
                .Select(s => s.Trim())
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .Where(i => i >= 0 && i < _hand.Count)
                .Distinct()
                .ToList();

            if (indices.Count == 0)
            {
                Console.WriteLine("Некорректный ввод.");
                return;
            }

            var selectedCards = indices.Select(i => _hand[i]).ToList();

            Console.Write("Какой номинал вы хотите объявить (Ten, Jack, Queen, King, Ace, Joker): ");
            var declaredNominal = Console.ReadLine();
            if (!Enum.TryParse<CardType>(declaredNominal, true, out var nominalType)) return;

            Console.Write("Сколько вы хотите объявить (число): ");
            if (!int.TryParse(Console.ReadLine(), out var declaredCount)) return;

            var packet = new byte[1 + 1 + selectedCards.Count + 1 + 1];
            var offset = 0;

            packet[offset++] = (byte)GameCommand.PlayCards;
            packet[offset++] = (byte)selectedCards.Count;
            foreach (var card in selectedCards)
            {
                packet[offset++] = (byte)card.Type;
            }
            packet[offset++] = (byte)nominalType;
            packet[offset++] = (byte)declaredCount;

            await _stream.WriteAsync(packet, 0, packet.Length);
        }

        private async Task HandleAccuse()
        {
            // Обвинять можно всегда, не проверяем ход
            var packet = new byte[1];
            packet[0] = (byte)GameCommand.AccuseLie;
            await _stream.WriteAsync(packet, 0, packet.Length);
        }

        private async Task HandleLeave()
        {
            var packet = new byte[1];
            packet[0] = (byte)GameCommand.Leave;
            await _stream.WriteAsync(packet, 0, packet.Length);
        }
    }