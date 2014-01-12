using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fanfouModel;

namespace FanfouBiz
{
    /// <summary>
    /// 饭否API
    /// </summary>
    public static class Fanfou
    {
        #region Fanfou API
        public static string ApiBase = "http://api.fanfou.com/";
        //public static string OAuth_ConsumerKey = "0e0ae1d4edc50be8ddbc6c0bcc83c309";//自定义consumer key
        //public static string OAuth_ConsumerSecret = "8a77d03b3d8665bf77225a02eab0e93b";//自定义consumer secret
        public static string OAuth_ConsumerKey = "";//自定义consumer key
        public static string OAuth_ConsumerSecret = "";//自定义consumer secret

        public static Dictionary<string, Api> OAuthProvider = new Dictionary<string, Api>()
        {
            {"requestToken",new Api{Name="requestToken",Action="http://fanfou.com/oauth/request_token",Method="GET"}},
            {"authorize",new Api{Name="authorize",Action="http://fanfou.com/oauth/authorize?oauth_callback=oob&oauth_token=",Method="GET"}},
            {"accessToken",new Api{Name="accessToken",Action="http://fanfou.com/oauth/access_token",Method="GET"}},
        };

        public static Dictionary<string, Api> Api =new Dictionary<string,Api>()
        {
            {"verifyCredentials",new Api{Name="verifyCredentials",Action="account/verify_credentials.json",Method="GET"}},
            {"queryApiLimit",new Api{Name="queryApiLimit",Action="account/rate_limit_status.xml",Method="GET"}},
            {"getPublicTimeline",new Api{Name="getPublicTimeline",Action="statuses/public_timeline.json",Method="GET"}},
            {"getUserTimeline",new Api{Name="getUserTimeline",Action="statuses/user_timeline.json",Method="GET"}},
            {"getFollowerList",new Api{Name="getFollowerList",Action="followers/ids.json",Method="GET"}},
            {"getFriendList",new Api{Name="getFriendList",Action="friends/ids.json",Method="GET"}},
            {"isFollowing",new Api{Name="isFollowing",Action="friendships/exists.json",Method="GET"}},
            {"showRelationshipById",new Api{Name="showRelationshipById",Action="friendships/show.json",Method="GET"}},            
            {"getUser",new Api{Name="getUser",Action="users/show.json",Method="GET"}},
            {"friendshipExists",new Api{Name="friendshipExists",Action="friendships/exists.json",Method="GET"}},
            {"postStatus",new Api{Name="postStatus",Action="statuses/update.json",Method="POST"}},
            {"postDirectMessage",new Api{Name="postDirectMessage",Action="direct_messages/new.json",Method="POST"}}
        };
        #endregion Fanfou API
    }
}
