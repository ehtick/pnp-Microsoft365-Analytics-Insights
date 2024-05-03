namespace Common.Entities.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class v105 : DbMigration
    {
        public override void Up()
        {
            Sql(IMPORT_LOG_SQL);
            Sql(PROVINCE_SQL);
            Sql(COUNTY_NAME_SQL);
            Sql(CITY_NAME_FIELD_SQL);
            Sql(CHANGE_URL_FIELD_SQL);
            Console.WriteLine("DB SCHEMA: Applied 'nvarchar field conversions' succesfully.");
        }

        public override void Down()
        {
        }

        #region Import Log

        const string IMPORT_LOG_SQL = @"
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_import_log
	(
	id int NOT NULL IDENTITY (1, 1),
	import_message varchar(250) NOT NULL,
	contents nvarchar(MAX) NOT NULL,
	machine_name varchar(50) NOT NULL,
	time_stamp datetime NOT NULL,
	hit_id int NULL,
	event_id uniqueidentifier NULL,
	search_id int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_import_log SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_import_log ON
GO
IF EXISTS(SELECT * FROM dbo.import_log)
	 EXEC('INSERT INTO dbo.Tmp_import_log (id, import_message, contents, machine_name, time_stamp, hit_id, event_id, search_id)
		SELECT id, import_message, CONVERT(nvarchar(MAX), contents), machine_name, time_stamp, hit_id, event_id, search_id FROM dbo.import_log WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_import_log OFF
GO
DROP TABLE dbo.import_log
GO
EXECUTE sp_rename N'dbo.Tmp_import_log', N'import_log', 'OBJECT' 
GO
ALTER TABLE dbo.import_log ADD CONSTRAINT
	PK_import_log PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT

";

        #endregion

        #region Province
        const string PROVINCE_SQL = @"
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_urls
	(
	id int NOT NULL IDENTITY (1, 1),
	full_url nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_urls SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_urls ON
GO
IF EXISTS(SELECT * FROM dbo.urls)
	 EXEC('INSERT INTO dbo.Tmp_urls (id, full_url)
		SELECT id, CONVERT(nvarchar(MAX), full_url) FROM dbo.urls WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_urls OFF
GO
ALTER TABLE dbo.audit_events
	DROP CONSTRAINT FK_events_urls
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_urls
GO
DROP TABLE dbo.urls
GO
EXECUTE sp_rename N'dbo.Tmp_urls', N'urls', 'OBJECT' 
GO
ALTER TABLE dbo.urls ADD CONSTRAINT
	PK_urls PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.audit_events ADD CONSTRAINT
	FK_events_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.audit_events SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_countries
	(
	id int NOT NULL IDENTITY (1, 1),
	country_name nvarchar(250) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_countries SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_countries ON
GO
IF EXISTS(SELECT * FROM dbo.countries)
	 EXEC('INSERT INTO dbo.Tmp_countries (id, country_name)
		SELECT id, CONVERT(nvarchar(250), country_name) FROM dbo.countries WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_countries OFF
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_countries
GO
DROP TABLE dbo.countries
GO
EXECUTE sp_rename N'dbo.Tmp_countries', N'countries', 'OBJECT' 
GO
ALTER TABLE dbo.countries ADD CONSTRAINT
	PK_countries PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_countries ON dbo.countries
	(
	country_name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_cities
	(
	id int NOT NULL IDENTITY (1, 1),
	city_name nvarchar(250) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_cities SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_cities ON
GO
IF EXISTS(SELECT * FROM dbo.cities)
	 EXEC('INSERT INTO dbo.Tmp_cities (id, city_name)
		SELECT id, CONVERT(nvarchar(250), city_name) FROM dbo.cities WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_cities OFF
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_cities
GO
DROP TABLE dbo.cities
GO
EXECUTE sp_rename N'dbo.Tmp_cities', N'cities', 'OBJECT' 
GO
ALTER TABLE dbo.cities ADD CONSTRAINT
	PK_cities PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_cities ON dbo.cities
	(
	city_name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_provinces
	(
	id int NOT NULL IDENTITY (1, 1),
	province_name nvarchar(250) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_provinces SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_provinces ON
GO
IF EXISTS(SELECT * FROM dbo.provinces)
	 EXEC('INSERT INTO dbo.Tmp_provinces (id, province_name)
		SELECT id, CONVERT(nvarchar(250), province_name) FROM dbo.provinces WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_provinces OFF
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_user_provinces
GO
DROP TABLE dbo.provinces
GO
EXECUTE sp_rename N'dbo.Tmp_provinces', N'provinces', 'OBJECT' 
GO
ALTER TABLE dbo.provinces ADD CONSTRAINT
	PK_provinces PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_provinces ON dbo.provinces
	(
	province_name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_cities FOREIGN KEY
	(
	city_id
	) REFERENCES dbo.cities
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_countries FOREIGN KEY
	(
	country_id
	) REFERENCES dbo.countries
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_user_provinces FOREIGN KEY
	(
	location_province_id
	) REFERENCES dbo.provinces
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
";

        #endregion

        #region Country
        const string COUNTY_NAME_SQL = @"
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_urls
	(
	id int NOT NULL IDENTITY (1, 1),
	full_url nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_urls SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_urls ON
GO
IF EXISTS(SELECT * FROM dbo.urls)
	 EXEC('INSERT INTO dbo.Tmp_urls (id, full_url)
		SELECT id, CONVERT(nvarchar(MAX), full_url) FROM dbo.urls WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_urls OFF
GO
ALTER TABLE dbo.audit_events
	DROP CONSTRAINT FK_events_urls
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_urls
GO
DROP TABLE dbo.urls
GO
EXECUTE sp_rename N'dbo.Tmp_urls', N'urls', 'OBJECT' 
GO
ALTER TABLE dbo.urls ADD CONSTRAINT
	PK_urls PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.audit_events ADD CONSTRAINT
	FK_events_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.audit_events SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_cities
	(
	id int NOT NULL IDENTITY (1, 1),
	city_name nvarchar(250) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_cities SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_cities ON
GO
IF EXISTS(SELECT * FROM dbo.cities)
	 EXEC('INSERT INTO dbo.Tmp_cities (id, city_name)
		SELECT id, CONVERT(nvarchar(250), city_name) FROM dbo.cities WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_cities OFF
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_cities
GO
DROP TABLE dbo.cities
GO
EXECUTE sp_rename N'dbo.Tmp_cities', N'cities', 'OBJECT' 
GO
ALTER TABLE dbo.cities ADD CONSTRAINT
	PK_cities PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_cities ON dbo.cities
	(
	city_name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_countries
	(
	id int NOT NULL IDENTITY (1, 1),
	country_name nvarchar(250) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_countries SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_countries ON
GO
IF EXISTS(SELECT * FROM dbo.countries)
	 EXEC('INSERT INTO dbo.Tmp_countries (id, country_name)
		SELECT id, CONVERT(nvarchar(250), country_name) FROM dbo.countries WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_countries OFF
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_countries
GO
DROP TABLE dbo.countries
GO
EXECUTE sp_rename N'dbo.Tmp_countries', N'countries', 'OBJECT' 
GO
ALTER TABLE dbo.countries ADD CONSTRAINT
	PK_countries PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_countries ON dbo.countries
	(
	country_name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_cities FOREIGN KEY
	(
	city_id
	) REFERENCES dbo.cities
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_countries FOREIGN KEY
	(
	country_id
	) REFERENCES dbo.countries
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
";
        #endregion

        #region City

        const string CITY_NAME_FIELD_SQL = @"
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_urls
	(
	id int NOT NULL IDENTITY (1, 1),
	full_url nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_urls SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_urls ON
GO
IF EXISTS(SELECT * FROM dbo.urls)
	 EXEC('INSERT INTO dbo.Tmp_urls (id, full_url)
		SELECT id, CONVERT(nvarchar(MAX), full_url) FROM dbo.urls WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_urls OFF
GO
ALTER TABLE dbo.audit_events
	DROP CONSTRAINT FK_events_urls
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_urls
GO
DROP TABLE dbo.urls
GO
EXECUTE sp_rename N'dbo.Tmp_urls', N'urls', 'OBJECT' 
GO
ALTER TABLE dbo.urls ADD CONSTRAINT
	PK_urls PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.audit_events ADD CONSTRAINT
	FK_events_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.audit_events SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_cities
	(
	id int NOT NULL IDENTITY (1, 1),
	city_name nvarchar(250) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_cities SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_cities ON
GO
IF EXISTS(SELECT * FROM dbo.cities)
	 EXEC('INSERT INTO dbo.Tmp_cities (id, city_name)
		SELECT id, CONVERT(nvarchar(250), city_name) FROM dbo.cities WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_cities OFF
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_cities
GO
DROP TABLE dbo.cities
GO
EXECUTE sp_rename N'dbo.Tmp_cities', N'cities', 'OBJECT' 
GO
ALTER TABLE dbo.cities ADD CONSTRAINT
	PK_cities PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_cities ON dbo.cities
	(
	city_name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_cities FOREIGN KEY
	(
	city_id
	) REFERENCES dbo.cities
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

";

        #endregion

        #region URL

        const string CHANGE_URL_FIELD_SQL = @"
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_urls
	(
	id int NOT NULL IDENTITY (1, 1),
	full_url nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_urls SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_urls ON
GO
IF EXISTS(SELECT * FROM dbo.urls)
	 EXEC('INSERT INTO dbo.Tmp_urls (id, full_url)
		SELECT id, CONVERT(nvarchar(MAX), full_url) FROM dbo.urls WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_urls OFF
GO
ALTER TABLE dbo.audit_events
	DROP CONSTRAINT FK_events_urls
GO
ALTER TABLE dbo.hits
	DROP CONSTRAINT FK_hits_urls
GO
DROP TABLE dbo.urls
GO
EXECUTE sp_rename N'dbo.Tmp_urls', N'urls', 'OBJECT' 
GO
ALTER TABLE dbo.urls ADD CONSTRAINT
	PK_urls PRIMARY KEY CLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.hits ADD CONSTRAINT
	FK_hits_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.hits SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.audit_events ADD CONSTRAINT
	FK_events_urls FOREIGN KEY
	(
	url_id
	) REFERENCES dbo.urls
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.audit_events SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

";

        #endregion
    }
}