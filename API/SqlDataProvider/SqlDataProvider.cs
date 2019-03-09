﻿using System;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using Microsoft.ApplicationBlocks.Data;

namespace DNNrocketAPI
{

	/// -----------------------------------------------------------------------------
	/// <summary>
	/// SQL Server implementation of the abstract DataProvider class
	/// </summary>
	/// -----------------------------------------------------------------------------
	public class SqlDataProvider : DataProvider
	{

		#region Private Members

		private const string ProviderType = "data";
		private string ModuleQualifier = "DNNrocket_";

		private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
		private readonly string _connectionString;
		private readonly string _providerPath;
		private readonly string _objectQualifier;
		private readonly string _databaseOwner;

		#endregion

		#region Constructors

		public SqlDataProvider()
		{

            // Read the configuration specific information for this provider
            Provider objProvider = (Provider)(_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);

			// Read the attributes for this provider

			//Get Connection string from web.config
			_connectionString = Config.GetConnectionString();

			if (string.IsNullOrEmpty(_connectionString))
			{
				// Use connection string specified in provider
				_connectionString = objProvider.Attributes["connectionString"];
			}

			_providerPath = objProvider.Attributes["providerPath"];

			_objectQualifier = objProvider.Attributes["objectQualifier"];
			if (!string.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_", StringComparison.Ordinal) == false)
			{
				_objectQualifier += "_";
			}

			_databaseOwner = objProvider.Attributes["databaseOwner"];
			if (!string.IsNullOrEmpty(_databaseOwner) && _databaseOwner.EndsWith(".", StringComparison.Ordinal) == false)
			{
				_databaseOwner += ".";
			}

		}

		#endregion

		#region Properties

		public string ConnectionString
		{
			get
			{
				return _connectionString;
			}
		}

		public string ProviderPath
		{
			get
			{
				return _providerPath;
			}
		}

		public string ObjectQualifier
		{
			get
			{
				return _objectQualifier;
			}
		}

		public string DatabaseOwner
		{
			get
			{
				return _databaseOwner;
			}
		}

		private string NamePrefix
		{
			get { return DatabaseOwner + ObjectQualifier + ModuleQualifier; }
		}

		#endregion

		#region Private Methods

		private static object GetNull(object Field)
		{
			return DotNetNuke.Common.Utilities.Null.GetNull(Field, DBNull.Value);
		}

        #endregion

        #region Public Methods

        public override int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", int systemId = -1)
        {
            var rtncount = 0;
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "GetListCount", portalId, moduleId, typeCode, sqlSearchFilter, lang, systemId, rtncount));
        }

        public override IDataReader GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, int systemId = -1)
        {
            return SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "GetList", portalId, moduleId, typeCode, sqlSearchFilter, sqlOrderBy, returnLimit, pageNumber, pageSize, recordCount, lang, systemId);
        }

        public override IDataReader GetInfo(int itemId, string lang = "")
        {
            return SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "Get", itemId, lang);
        }

        public override int Update(int ItemId, int PortalId, int ModuleId, String TypeCode, String XMLData, String GUIDKey, DateTime ModifiedDate, String TextData, int XrefItemId, int ParentItemId, int UserId, string Lang, int systemId)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "Update", ItemId, PortalId, ModuleId, TypeCode, XMLData, GUIDKey, ModifiedDate, TextData, XrefItemId, ParentItemId, UserId, Lang, systemId));
        }

        public override void Delete(int ItemID)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "Delete", ItemID);
        }

        public override void CleanData()
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "CleanData");
        }

        public override IDataReader GetRecord(int itemId)
        {
            return SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "GetRecord", itemId);
        }

        public override IDataReader GetRecordLang(int parentitemId, String lang)
        {
            return SqlHelper.ExecuteReader(ConnectionString, DatabaseOwner + ObjectQualifier + ModuleQualifier + "GetRecordLang", parentitemId, lang);
        }

        #endregion




	}

}