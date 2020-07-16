﻿using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNrocketAPI.Componants
{
    public class CommandSecurity
    {
        private int _userId;
        private UserInfo _userInfo;
        static ConcurrentDictionary<string, bool> _commandSecurity;  // thread safe dictionary.
        private RocketInterface _interfaceInfo;
        private int _tabid;
        private int _moduleid;

        public CommandSecurity(RocketInterface interfaceInfo)
        {
            _commandSecurity = new ConcurrentDictionary<string, bool>();
            _tabid = -1;
            _moduleid = -1;
            _interfaceInfo = interfaceInfo;
            _userInfo = UserController.Instance.GetCurrentUserInfo();
            ValidateUser();
        }


        public CommandSecurity(int tabId, int moduleId, RocketInterface interfaceInfo)
        {
            _commandSecurity = new ConcurrentDictionary<string, bool>();
            _tabid = tabId;
            _moduleid = moduleId;
            _interfaceInfo = interfaceInfo;
            _userInfo = UserController.Instance.GetCurrentUserInfo();
            ValidateUser();
        }

        public CommandSecurity(int portalid, int userid, int tabId, int moduleId, RocketInterface interfaceInfo)
        {
            _commandSecurity = new ConcurrentDictionary<string, bool>();
            _tabid = tabId;
            _moduleid = moduleId;
            _interfaceInfo = interfaceInfo;
            _userInfo = UserController.Instance.GetUserById(portalid, userid);
            ValidateUser();
        }

        private void ValidateUser()
        {
            if (_userInfo != null && _userInfo.UserID > 0 && !_userInfo.IsDeleted && _userInfo.Membership.Approved)
            {
                ValidUser = true;
                _userId = _userInfo.UserID;
            }
            else
            {
                ValidUser = false;
                _userId = -1;
            }
        }

        public int TabId { get { return _tabid; } }
        public int ModuleId { get { return _moduleid; } }

        public bool ValidUser { get; private set; }

        public int UserId { get { return _userId; } }

        public bool SecurityCheckIsSuperUser()
        {
            if (!ValidUser) return false;
            return _userInfo.IsSuperUser;
        }


        public void SignUserOut()
        {
            var ps = new PortalSecurity();
            ps.SignOut();
        }

        public void AddCommandList(string commandKeyCSV, bool requiresSecurity)
        {
            var clist = commandKeyCSV.Split(',');
            foreach (var c in clist)
            {
                AddCommand(c, requiresSecurity);
            }
        }


        public void AddCommand(string commandKey,bool requiresSecurity)
        {
            if (_commandSecurity.ContainsKey(commandKey)) RemoveCommand(commandKey);
            if (!_commandSecurity.ContainsKey(commandKey))
            {
                _commandSecurity.AddOrUpdate(commandKey, requiresSecurity, (key, existingVal) => {return existingVal;});
            }
        }

        public void RemoveCommand(string commandKey)
        {
            bool v;
            if (!_commandSecurity.TryRemove(commandKey, out v))
            {
                // should not fail, but ignore if does.
            }

        }

        public bool ValidCommand(string commandKey)
        {
            return _commandSecurity.ContainsKey(commandKey);
        }

        public string DefaultCommandInMenu()
        {
            foreach (var c in _commandSecurity)
            {
                if (c.Value) return c.Key;
            }
            return _interfaceInfo.DefaultCommand;
        }

        public bool HasSecurityAccess(string commandKey)
        {
            if (SecurityCheckIsSuperUser()) return true;
            if (!_commandSecurity.ContainsKey(commandKey) || !_commandSecurity[commandKey])
            {
                return true;
            }
            if (_userInfo == null) return false;
            if (HasModuleEditRights() && _interfaceInfo.SecurityCheckUser(_userInfo.PortalID, _userId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SecureCommand(string commandKey)
        {
            if (!_commandSecurity.ContainsKey(commandKey)) return true; 
            return _commandSecurity[commandKey]; 
        }

        public bool HasModuleEditRights()
        {
           try
            {
                if (_tabid == -1 && _moduleid == -1) return true;
                if (_tabid == 0) return false;
                if (_moduleid == 0) return false;
                var moduleInfo = ModuleController.Instance.GetModule(_moduleid, _tabid, false);
                if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "MANAGE", moduleInfo))
                {
                    return true;
                }
            }
            catch (Exception exc) 
            {
                // We may not have all infomration from DNN to do this, so just return false.
                var msg = exc.ToString();
                return false;
            }
            return false;
        }


    }
}
