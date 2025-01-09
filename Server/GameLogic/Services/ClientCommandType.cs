/*
 * ClientCommandType.cs
 * Description: Constants for the different types of commands that can be sent from the client to the server
 */

public static class ClientCommandType
{
    public const string CLIENT_LOBBY_JOIN = "CLIENT_LOBBY_JOIN";
    public const string CLIENT_LOBBY_LEAVE = "CLIENT_LOBBY_LEAVE";
    public const string CLIENT_LOBBY_READY = "CLIENT_LOBBY_READY";
    public const string CLIENT_LOBBY_UNREADY = "CLIENT_LOBBY_UNREADY";
    public const string CLIENT_LOBBY_CHANGE_NAME = "CLIENT_LOBBY_CHANGE_NAME";
    public const string CLIENT_GAME_MOVE = "CLIENT_GAME_MOVE";
    public const string CLIENT_GAME_BOMB = "CLIENT_GAME_BOMB";
}