using System.Text.Json;

namespace TwitchSharp.Entitys
{
    public class AuthorizedTwitchUser : TwitchUser
    {
        /*
        !!! NOT IMPLEMENTED YET !!!
        !!! NOT IMPLEMENTED YET !!!
        !!! NOT IMPLEMENTED YET !!!

        The Authorized Twitch User will be created by using an User Refresh Token, which will be needed to get user specific data,
        which is not availible for normal users and moderators, such as a list of moderators, vips, ...

        !!! NOT IMPLEMENTED YET !!!
        !!! NOT IMPLEMENTED YET !!!
        !!! NOT IMPLEMENTED YET !!!
        */
        public AuthorizedTwitchUser(TwitchClient client, JsonElement data) : base(client, data)
        {
            throw new NotImplementedException();
        }
    }
}