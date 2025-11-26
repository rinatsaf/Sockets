namespace Server;

public enum GameCommand : byte
{
    // Подключение
    Join = 0x20,      
    Ready = 0x21,
    Leave = 0x22,
    
    // Ходы
    PlayCards = 0x30,  
    AccuseLie = 0x31,  

    // Ответы от сервера
    GameState = 0x40,  
    GameOver = 0x41,  
    Error = 0x42       
}