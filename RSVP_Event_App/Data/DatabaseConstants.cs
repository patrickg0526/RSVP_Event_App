namespace RSVP_Event_App.Data;

public static class DatabaseConstants
{
    public const string DatabaseFilename = "RsvpApp.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite
        // create the database if it doesn't exist
        | SQLite.SQLiteOpenFlags.Create
        // enable multi-threaded database access
        | SQLite.SQLiteOpenFlags.SharedCache;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}
