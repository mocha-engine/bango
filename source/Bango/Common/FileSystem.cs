using System.Text.Json;

namespace Bango;
public class FileSystem
{
	public static FileSystem Main => new FileSystem( "content\\" );

	private List<FileSystemWatcher> _watchers = new();
	private string BasePath { get; }

	public FileSystem( string relativePath )
	{
		this.BasePath = Path.GetFullPath( relativePath, Directory.GetCurrentDirectory() );
	}

	public string GetAbsolutePath( string relativePath, bool ignorePathNotFound = false )
	{
		var path = Path.Combine( this.BasePath, relativePath ).NormalizePath();

		switch ( Path.GetExtension( relativePath ) )
		{
			case ".mmdl":
			case ".mmat":
			case ".mtex":
				path += "_c"; // Load compiled assets
				break;
		}

		if ( !File.Exists( path ) && !Directory.Exists( path ) && !ignorePathNotFound )
			Log.Warning( $"Path not found: {path}. Continuing anyway." );

		return path;
	}

	public string ReadAllText( string relativePath )
	{
		return File.ReadAllText( GetAbsolutePath( relativePath ) );
	}

	public byte[] ReadAllBytes( string relativePath )
	{
		return File.ReadAllBytes( GetAbsolutePath( relativePath ) );
	}

	public FileStream OpenRead( string relativePath )
	{
		return File.OpenRead( GetAbsolutePath( relativePath ) );
	}

	public bool Exists( string relativePath )
	{
		return File.Exists( GetAbsolutePath( relativePath, ignorePathNotFound: true ) );
	}

	public FileSystemWatcher CreateWatcher( string relativeDir, string filter, Action onChange )
	{
		var directoryName = GetAbsolutePath( relativeDir );
		var watcher = new FileSystemWatcher( directoryName, filter );

		watcher.NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size;

		watcher.EnableRaisingEvents = true;
		watcher.IncludeSubdirectories = true;
		watcher.Changed += ( _, e ) =>
		{
			onChange();
		};

		_watchers.Add( watcher );

		return watcher;
	}

	public string GetFullPath( string filePath )
	{
		return Path.Combine( BasePath, filePath );
	}

	public IEnumerable<string> GetFiles( string directory )
	{
		return Directory.GetFiles( GetAbsolutePath( directory ) );
	}

	public IEnumerable<string> GetDirectories( string directory )
	{
		return Directory.GetDirectories( GetAbsolutePath( directory ) );
	}

	public string GetRelativePath( string filePath )
	{
		return Path.GetRelativePath( BasePath, filePath );
	}

	public T? Deserialize<T>( string filePath )
	{
		var text = ReadAllText( filePath );
		return JsonSerializer.Deserialize<T>( text );
	}

	public bool IsFileReady( string relativePath )
	{
		var path = GetAbsolutePath( relativePath );

		if ( !Path.Exists( path ) )
		{
			throw new Exception( "File does not exist" );
		}

		try
		{
			using FileStream inputStream = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.None );
			return inputStream.Length > 0;
		}
		catch ( Exception )
		{
			return false;
		}
	}
}
