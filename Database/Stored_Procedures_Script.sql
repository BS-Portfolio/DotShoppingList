USE ShoppingListStore;
GO

DROP PROCEDURE IF EXISTS uspRemoveUser;
GO

CREATE PROCEDURE uspRemoveUser
	@userId UNIQUEIDENTIFIER,
	@listAdminRoleEnumIndex INT,
	@success BIT OUTPUT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

	BEGIN TRANSACTION;

	BEGIN TRY

	DECLARE @affected_rows INT;

	-- declare iteration parameters
	DECLARE @shoppingListIDasAdmin UNIQUEIDENTIFIER;

	-- get admin shopping list id's
	DECLARE shopping_list_cursor CURSOR FOR
		SELECT ShoppingListID FROM ListMember AS LM
		JOIN ListUser AS LS ON LM.UserID = LS.UserID
		WHERE LS.UserID = @userId AND LM.UserRoleID IN (
			SELECT UserRoleID FROM UserRole WHERE UserRole.UserRoleID = @listAdminRoleEnumIndex
			)

	-- remove list items, list member entries and shopping list entries
	FETCH NEXT FROM shopping_list_cursor INTO @shoppingListIDasAdmin;

	WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM Item 
			WHERE Item.ShoppingListID = @shoppingListIDasAdmin;

			DELETE FROM ListMember
			WHERE ListMember.ShoppingListID = @shoppingListIDasAdmin;

			DELETE FROM ShoppingList
			WHERE ShoppingListID = @shoppingListIDasAdmin;

			FETCH NEXT FROM shopping_list_cursor INTO @shoppingListIDasAdmin;
		END;

	CLOSE shopping_list_cursor;
	DEALLOCATE shopping_list_cursor;

	-- remove user from lists as collaborator

	DELETE FROM ListMember WHERE ListMember.UserID = @userId;

	-- remove User

	DELETE FROM ListUser WHERE UserID = @userId;

	COMMIT TRANSACTION;



	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION;
		DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT  
			SELECT  
				@ErrorMessage = ERROR_MESSAGE(),  
				@ErrorSeverity = ERROR_SEVERITY(),  
				@ErrorState = ERROR_STATE()  

			-- Rethrow the error  
			RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
	END CATCH
END;


