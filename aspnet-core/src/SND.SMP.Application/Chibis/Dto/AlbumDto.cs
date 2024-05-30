using System;
using System.Collections.Generic;
using SND.SMP.FileUploadAPI.Dto;

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

public class GetAlbumDto
{
    public string message { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public bool isNsfw { get; set; }
    public int count { get; set; }
    public List<File> files { get; set; }
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
    public List<File> files { get; set; } = [];
}

public class CreateAlbum
{
    public string uuid { get; set; }
    public string name { get; set; }
    public DateTime createdAt { get; set; }
}