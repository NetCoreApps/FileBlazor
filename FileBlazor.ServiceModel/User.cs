﻿using ServiceStack;

namespace FileBlazor.ServiceModel;

[ValidateIsAuthenticated]
[Tag("User")]
public class GetUserProfile : IGet, IReturn<GetUserProfileResponse> {}
public class GetUserProfileResponse
{
    public UserProfile Result { get; set; }
    public ResponseStatus ResponseStatus { get; set; }
}

public class UserProfile
{
    public string DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Handle { get; set; }
}

[ValidateIsAuthenticated]
[Tag("User")]
public class GetUserInfo : IGet, IReturn<GetUserInfoResponse>
{
    public string RefId { get; set; }
}
public class GetUserInfoResponse
{
    public UserResult Result { get; set; }
    public ResponseStatus ResponseStatus { get; set; }
}


public class Likes
{
    public List<int> ArtifactIds { get; set; }
    public List<int> AlbumIds { get; set; }
}
public class UserResult
{
    public int Id { get; set; }
    public string RefId { get; set; }
    public string? Handle { get; set; }
    public string? Avatar { get; set; }
    public string? ProfileUrl { get; set; }
}

