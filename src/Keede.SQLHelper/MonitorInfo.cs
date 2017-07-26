namespace Keede.SQLHelper
{
    /// <summary>
    ///
    /// </summary>
    public class MonitorInfo : DbExceptionInfo
    {
        /// <summary>
        ///
        /// </summary>
        public static string VerfityTableSQL
        {
            get
            {
                return @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MonitorSQLQuery]') AND type in (N'U'))
	BEGIN
		SELECT 1
	END
ELSE
	BEGIN
		SELECT 0
	END
";
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static string CreateTableSQL
        {
            get
            {
                return @"
CREATE TABLE [dbo].[MonitorSQLQuery](
	[ID] [uniqueidentifier] NOT NULL,
    [RequestFrom] [varchar](128) NULL,
	[QueryString] [varchar](max) NULL,
	[ParameterString] [varchar](max) NULL,
	[ExceptionMessage] [varchar](max) NULL,
	[TimeConsuming] [bigint] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_MonitorSQLQuery] PRIMARY KEY CLUSTERED
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
ALTER TABLE [dbo].[MonitorSQLQuery] ADD  CONSTRAINT [DF_MonitorSQLQuery_TimeConsuming]  DEFAULT ((0)) FOR [TimeConsuming]
ALTER TABLE [dbo].[MonitorSQLQuery] ADD  CONSTRAINT [DF_MonitorSQLQuery_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
";
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static string InsertSQL
        {
            get
            {
                return @"
INSERT INTO [MonitorSQLQuery]
           ([ID]
            ,[RequestFrom]
           ,[QueryString]
           ,[ParameterString]
           ,[ExceptionMessage]
           ,[TimeConsuming])
     VALUES
           (NEWID()
            ,@RequestFrom
           ,@QueryString
           ,@ParameterString
           ,@ExceptionMessage
           ,@TimeConsuming)
";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="timeConsuming"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        public MonitorInfo(long timeConsuming, string commandText, params Parameter[] parameters)
            : base(null, commandText, parameters)
        {
            TimeConsuming = timeConsuming;
        }

        /// <summary>
        ///
        /// </summary>
        public long TimeConsuming { get; set; }
    }
}