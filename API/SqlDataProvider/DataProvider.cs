﻿using System.Collections.Generic;
using System.Data;
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using System.Configuration;
using Simplisity;

namespace DNNrocketAPI
{

	/// -----------------------------------------------------------------------------
	/// <summary>
	/// An abstract class for the data access layer
	/// </summary>
	/// -----------------------------------------------------------------------------
	public abstract class DataProvider
	{

		#region Shared/Static Methods

		private static DataProvider provider;

		// return the provider
		public static DataProvider Instance()
		{
			if (provider == null)
			{
                const string assembly = "DNNrocketAPI.SqlDataProvider,DNNrocketAPI";
				Type objectType = Type.GetType(assembly, true, true);

				provider = (DataProvider)Activator.CreateInstance(objectType);
				DataCache.SetCache(objectType.FullName, provider);
			}

			return provider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not returning class state information")]
		public static IDbConnection GetConnection()
		{
			const string providerType = "data";
			ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(providerType);

			Provider objProvider = ((Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);
			string _connectionString;
			if (!String.IsNullOrEmpty(objProvider.Attributes["connectionStringName"]) && !String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]]))
			{
				_connectionString = System.Configuration.ConfigurationManager.AppSettings[objProvider.Attributes["connectionStringName"]];
			}
			else
			{
				_connectionString = objProvider.Attributes["connectionString"];
			}

			IDbConnection newConnection = new System.Data.SqlClient.SqlConnection();
			newConnection.ConnectionString = _connectionString.ToString();
			newConnection.Open();
			return newConnection;
		}

        #endregion


        #region "Abstract Methods"

        public abstract int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", string tableName = "DNNrocket");
        public abstract IDataReader GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string tableName = "DNNrocket");
        public abstract IDataReader GetInfo(int itemId, string lang = "", string tableName = "DNNrocket");
        public abstract int Update(int ItemId, int PortalId, int ModuleId, String TypeCode, String XMLData, String GUIDKey, DateTime ModifiedDate, String TextData, int XrefItemId, int ParentItemId, int UserId, string lang, int sortOrder, string tableName = "DNNrocket");
        public abstract void Delete(int itemId, string tableName = "DNNrocket");
        public abstract void CleanData(string tableName = "DNNrocket");
        public abstract void DeleteAllData(string tableName);
        public abstract IDataReader GetRecord(int itemId, string tableName = "DNNrocket");
        public abstract IDataReader GetRecordLang(int parentitemId,String lang, string tableName = "DNNrocket");
        public abstract IDataReader GetUsersCMS(int portalId, string sqlSearchFilter = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0);
        public abstract int GetUsersCountCMS(int portalId, string sqlSearchFilter = "");

        #endregion


    }

}