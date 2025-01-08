/*
 * ServerCommandType.cs
 * Description: Constants for the different types of commands that can be sent from the server to the client
 */

public static class ServerCommandType {
    public const string SERVER_LOBBY_JOIN = "SERVER_LOBBY_JOIN";
    public const string SERVER_LOBBY_LEAVE = "SERVER_LOBBY_LEAVE";
    public const string SERVER_LOBBY_UPDATE = "SERVER_LOBBY_UPDATE";
    public const string SERVER_GAME_START = "SERVER_GAME_START";
    public const string SERVER_GAME_UPDATE = "SERVER_GAME_UPDATE";
}