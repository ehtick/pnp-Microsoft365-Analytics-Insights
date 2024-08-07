INSERT INTO users(user_name)
	SELECT distinct imports.user_name 
	FROM ${STAGING_TABLE_ACTIVITY} imports
	left join users on users.user_name = imports.user_name
	where users.user_name is null;

INSERT INTO event_file_ext(extension_name)
	SELECT distinct imports.extension_name 
	FROM ${STAGING_TABLE_ACTIVITY} imports
	left join event_file_ext on event_file_ext.extension_name = imports.extension_name
	where event_file_ext.extension_name is null and imports.extension_name is not null and imports.extension_name != '';

INSERT INTO event_file_names(file_name)
	SELECT distinct imports.file_name 
	FROM ${STAGING_TABLE_ACTIVITY} imports
	left join event_file_names on event_file_names.file_name = imports.file_name
	where event_file_names.file_name is null and imports.file_name is not null and imports.file_name != '';

INSERT INTO event_operations(operation_name)
	SELECT distinct imports.operation_name 
	FROM ${STAGING_TABLE_ACTIVITY} imports
	left join event_operations on event_operations.operation_name = imports.operation_name
	where event_operations.operation_name is null and imports.operation_name is not null;



-- Insert unique SharePoint/OneDrive URLs from staging (events with URLs not null and don't already exist)
INSERT INTO urls(full_url)
	SELECT distinct imports.object_id 
	FROM ${STAGING_TABLE_ACTIVITY} imports
	left join 
		urls on urls.full_url = imports.object_id
	where 
		imports.object_id is not null AND imports.object_id != '' AND
		(imports.workload = 'SharePoint' OR imports.workload = 'OneDrive') AND
		not exists(select top 1 full_url from urls where full_url = imports.object_id)

-- Type names
INSERT INTO event_types(type_name)
	SELECT distinct imports.type_name 
	FROM ${STAGING_TABLE_ACTIVITY} imports
	left join event_types on event_types.type_name = imports.type_name
	where 
		event_types.type_name is null and 
		imports.type_name is not null 
		AND (imports.workload = 'SharePoint' OR imports.workload = 'OneDrive');


-- Insert common
insert into audit_events (id, user_id, operation_id, time_stamp, event_data)
	SELECT imports.[log_id]
		  ,users.id as userId
		  ,event_operations.id as opId
		  ,[time_stamp]
		  ,[event_data]
	  FROM ${STAGING_TABLE_ACTIVITY} imports
	  inner join users on users.user_name = imports.user_name
	  inner join event_operations on event_operations.operation_name = imports.operation_name


-- Insert SharePoint
insert into event_meta_sharepoint(event_id, file_name_id, file_extension_id, item_type_id, url_id)
	SELECT imports.[log_id]
		  ,event_file_names.id as fileNameId
		  ,event_file_ext.id as fileExtId
		  ,event_types.id as typeId
		  ,urls.id as urlId
	  FROM ${STAGING_TABLE_ACTIVITY} imports
	  left join event_file_names on event_file_names.file_name = imports.file_name
	  left join event_file_ext on event_file_ext.extension_name = imports.extension_name
	  inner join urls on urls.full_url = imports.object_id
	  left join event_types on event_types.type_name = imports.type_name
	where (imports.workload = 'SharePoint' OR imports.workload = 'OneDrive')


-- Insert Exchange
insert into event_meta_exchange(event_id, object_id)
	SELECT imports.[log_id]
		  ,imports.object_id
	  FROM ${STAGING_TABLE_ACTIVITY} imports
	  left join event_meta_exchange existing on existing.event_id = imports.log_id
	where imports.[workload] = 'Exchange'

-- Insert Azure AD
insert into event_meta_azure_ad(event_id)
	SELECT imports.[log_id]
	  FROM ${STAGING_TABLE_ACTIVITY} imports
	where imports.[workload] = 'AzureActiveDirectory'


-- Insert General
insert into event_meta_general(event_id, workload)
	SELECT imports.[log_id]
		  ,imports.workload
	  FROM ${STAGING_TABLE_ACTIVITY} imports
	  left join event_meta_general existing on existing.event_id = imports.log_id
	where imports.[workload] != 'Exchange'
		and imports.[workload] != 'SharePoint'
		and imports.[workload] != 'AzureActiveDirectory'
		and imports.[workload] != 'MicrosoftTeams'
