namespace Api.DTOs.Account
{
    public class FacebookResultDto
    {
        public FacebookData Data { get; set; }
    }

    public class FacebookData
    {
        public bool Is_Valid { get; set; }
        public string user_id { get; set; }
    }
}
