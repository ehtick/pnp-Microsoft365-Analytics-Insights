
-- Insert missing search terms
INSERT INTO search_terms(search_term)
	SELECT distinct imports.search_term 
	FROM ${STAGING_TABLE_SEARCHES} imports
	left join search_terms on search_terms.search_term = imports.search_term
	where search_terms.search_term is null;

--sp_updatestats

-- Insert searches from staging
insert into searches (
		session_id, 
		search_term_id,
		date_time
	)
	select  
		[sessions].id as sessionId, 
		search_terms.id as searchTermId,
		imports.date_time
	FROM ${STAGING_TABLE_SEARCHES} imports
		inner join users on users.user_name = imports.[user_name]
		inner join [sessions] on lower([sessions].ai_session_id) = lower(imports.ai_session_id) and [sessions].user_id = users.id
		inner join search_terms on search_terms.search_term = imports.search_term
		left join searches duplicateSearches on duplicateSearches.search_term_id = search_terms.id and duplicateSearches.[date_time] = imports.[date_time]
	where duplicateSearches.id is null
