using System;
using System.Collections.Generic;

public class AlbumDto
{
    public string message { get; set; }
    public List<Album> albums { get; set; }
}

public class CreateAlbumDto
{
    public string message { get; set; }
    public CreateAlbum album { get; set; }
}


public class Album
{
    public string uuid { get; set; }
    public string name { get; set; }
    public object description { get; set; }
    public bool nsfw { get; set; }
    public object zippedAt { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime editedAt { get; set; }
    public string cover { get; set; }
    public int count { get; set; }
}

public class CreateAlbum
{
    public string uuid { get; set; }
    public string name { get; set; }
    public DateTime createdAt { get; set; }
}