namespace WebApi.Dtos;

public record UserData(string DisplayNameFromGraph, string EmailFromGraph, string PreferredNameFromClaim, string EmailFromClaim);
