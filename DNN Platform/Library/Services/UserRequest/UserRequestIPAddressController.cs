﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Controllers;
using System;
using System.Web;
using System.Linq;
using DotNetNuke.Framework;
using System.Net;
using System.Net.Sockets;

namespace DotNetNuke.Services.UserRequest
{
    public class UserRequestIPAddressController : ServiceLocator<IUserRequestIPAddressController,UserRequestIPAddressController>, IUserRequestIPAddressController
    {
        public string GetUserRequestIPAddress(HttpRequestBase request)
        {
            return GetUserRequestIPAddress(request, IPAddressFamily.IPv4);
        }

        public string GetUserRequestIPAddress(HttpRequestBase request, IPAddressFamily ipFamily)
        {
            var userRequestIPHeader = HostController.Instance.GetString("UserRequestIPHeader", "X-Forwarded-For");
            var userIPAddress = string.Empty;

            if (request.Headers.AllKeys.Contains(userRequestIPHeader))
            {
                userIPAddress = request.Headers[userRequestIPHeader];
                userIPAddress = userIPAddress.Split(',')[0];                
            }            

            if (string.IsNullOrEmpty(userIPAddress))
            {
                var remoteAddrVariable = "REMOTE_ADDR";
                if (request.ServerVariables.AllKeys.Contains(remoteAddrVariable))
                {
                    userIPAddress = request.ServerVariables[remoteAddrVariable];
                }
            }

            if (string.IsNullOrEmpty(userIPAddress))
            {
                userIPAddress = request.UserHostAddress;
            }

            if (string.IsNullOrEmpty(userIPAddress) || userIPAddress.Trim() == "::1")
            {
                userIPAddress = string.Empty;
            }
            
            if (!string.IsNullOrEmpty(userIPAddress) && !ValidateIP(userIPAddress, ipFamily))
            {
                userIPAddress = string.Empty;
            }

            return userIPAddress;
        }

        private bool ValidateIP(string ipString, IPAddressFamily ipFamily)
        {
            IPAddress address;
            if (IPAddress.TryParse(ipString, out address))
            {
                if (ipFamily == IPAddressFamily.IPv4 &&
                    address.AddressFamily == AddressFamily.InterNetwork && 
                    ipString.Split('.').Length == 4)
                {
                    return true;
                }

                if (ipFamily == IPAddressFamily.IPv6 && 
                    address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return true;
                }
            }
            return false;
        }


        protected override Func<IUserRequestIPAddressController> GetFactory()
        {
            return () => new UserRequestIPAddressController();
        }
    }
}
