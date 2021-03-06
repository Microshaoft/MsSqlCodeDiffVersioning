﻿namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using System;
    using System.Composition;
    using System.Data;
    using System.Data.Common;

    [Export(typeof(IStoreProcedureExecutable))]
    public class NpgSQLStoreProcedureExecutorCompositionPlugin
                        : IStoreProcedureExecutable
                            , IParametersDefinitionCacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
                        _executor = new NpgSqlStoreProceduresExecutor();
        public string DataBaseType => "npgsql";////this.GetType().Name;
        public int CachedParametersDefinitionExpiredInSeconds
        {
            get;
            set;
        }
        public bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get;
            set;
        }
        public bool Execute
                    (
                        string connectionString
                        , string storeProcedureName
                        , JToken parameters
                        , out JToken result
                        , Func
                            <
                                IDataReader
                                , Type        // fieldType
                                , string    // fieldName
                                , int       // row index
                                , int       // column index
                                , JProperty   //  JObject Field 对象
                            > onReadRowColumnProcessFunc = null
                        , int commandTimeoutInSeconds = 90
                    )
        {
            if
                (
                    CachedParametersDefinitionExpiredInSeconds > 0
                    &&
                    _executor
                        .CachedParametersDefinitionExpiredInSeconds
                    !=
                    CachedParametersDefinitionExpiredInSeconds
                )
            {
                _executor
                        .CachedParametersDefinitionExpiredInSeconds
                            = CachedParametersDefinitionExpiredInSeconds;
            }
            result = null;
            DbConnection connection = new NpgsqlConnection(connectionString);
            result = _executor
                            .Execute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , onReadRowColumnProcessFunc
                                        , commandTimeoutInSeconds
                                    );
            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                _executor
                    .RefreshCachedExecuted
                        (
                            connection
                            , storeProcedureName
                        );
            }
            return true;
        }
    }
}
