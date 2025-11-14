using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Core.Service
{
    public static class ApiResponseMessage
    {

        public const string common_user_id_null = "Attention: User ID is null or not valid.";
        public const string user_registration_successfull = "IMPORTANT: Secure Your Account - Confirm Your Email Address.";
        public const string user_shopify_install_success = "Successfully login or install";
        public const string user_registration_by_email_verification = "SUCCESS: User created successfully.";
        public const string user_registration_Not_verified = "Attention: Check your email to re-verify your account.";
        public const string user_registration_failed = "ERROR: Registration failed. Please retry.";
        public const string user_third_party_not_match = "Attention: Your key does not match.";
        public const string user_registration_usertoke_failed = "ERROR: Unable to save user token.";
        public const string user_registration_device_failed = "ERROR: Unable to save user device.";
        public const string user_registration_email_failed = "ERROR: This email already exists on our server.";
        public const string user_registration_email_is_active = "Attention: This email already exists and the account is verified.";
        public const string user_registration_defaultsetting_failed = "ERROR: Subscription type for default not found.";
        public const string default_plan_types_getting_failed = "ERROR: Subscription type for default not found.";
        public const string user_incorrect_username_password = "Attention: Incorrect username or password.";
        public const string user_incorrect_email = "Attention: Incorrect username or password.";
        public const string user_can_not_find = "Attention: User is not signed in or token has expired.";
        public const string user_email_null = "Attention: Please provide an email ID.";
        public const string user_incorrect_email_format = "Attention: Email format is not valid or whitespace is not allowed! Example: example@someone.com";
        public const string user_incorrect_email_notExists = "Sorry, we couldn't find an account with those credentials.";
        public const string user_incorrect_email_account_not_verified = "Attention: Your account is not verified or a password is not set.";
        public const string user_incorrect_email_account_blocked = "Attention: Your account is blocked. Please contact the company helpline.";
        public const string free_download_limit_not_available = "Attention: Sign in to download all files.";


        public const string user_email_password_empty = "Attention: Username or password is empty.";
        public const string user_google_jwt_token = "Attention: Provided token is null or invalid.";
        public const string user_credentials_invalid = "Attention: Invalid credentials. Please try again.";
        public const string user_credentials_previous_password = "Attention: Invalid previous password. Please try again.";
        public const string user_successfully_login = "SUCCESS: Successfully logged in.";

        public const string servicetype_get_successfully = "SUCCESS: Successfully retrieved all service types.";
        public const string user_info_getting_successfully = "SUCCESS: Successfully retrieved user info.";
        public const string user_profile_update_successfully = "SUCCESS: User profile updated successfully.";
        public const string menu_list_success_message = "SUCCESS: Successfully retrieved all menu lists.";
        public const string promotion_list_success_message = "SUCCESS: Successfully retrieved all promotions.";
        public const string promotion_user_update = "SUCCESS: User promotion updated successfully.";
        public const string promotion_user_insert = "SUCCESS: User promotion inserted successfully.";
        public const string order_charge_break_down_success_message = "SUCCESS: Successfully retrieved all data.";
        public const string common_update_success_message = "SUCCESS: All data updated successfully.";
        public const string common_update_failed_message = "FAILED: Nothing update.";
        public const string default_setting_list_success_message = "SUCCESS: Successfully retrieved all default settings lists.";
        public const string order_image_detail_no_working_list = "SUCCESS: Successfully retrieved all image details that are not working.";
        public const string order_image_detail_successfully_get_data = "SUCCESS: Successfully retrieved all pending image details.";
        public const string menu_list_empty_message = "Menu list is empty.";
        public const string menu_list_error_message = "ERROR: Something went wrong with the menu list.";
        public const string promotion_list_error_message = "ERROR: Something went wrong with the promotion list.";
        public const string promotion_list_user_error_logged_message = "ERROR: A user needs to be logged in to avail the offer.";
        public const string token_expire_message = "Attention: Your token has expired. Please login to your account with your credentials.";
        public const string user_promotion_list_user_error_logged_message = "ERROR: You cannot provide promotion settings with null data.";
        public const string user_promotion_not_found = "ERROR: The provided promotion ID is not valid or data not found.";
        public const string common_see_try_catch = "ERROR: Something went wrong. Please check the try-catch block.";
        public const string default_setting_list_try_catch_message = "ERROR: Something went wrong in the try-catch exception.";
        public const string default_setting_list_error_message = "ERROR: Something went wrong with the default settings list.";
        public const string order_image_detail_try_catch_message = "ERROR: Something went wrong. Please check the method.";
        public const string service_default_type_for_automated_not_true = "Attention: Please update the 'is_default_for_automated' field to 'true' in the database.";
        public const string order_image_detail_not_availabe_for_automated = "Attention: No available files for automated working.";
        public const string menu_list_error_nocount = "Attention: No menus found.";
        public const string free_download_file_count_null = "Attention: No downloadable files available.";
        public const string promotion_list_error_nocount = "Attention: No promotions available yet.";
        public const string default_setting_list_error_nocount = "Attention: No default settings found.";
        public const string service_list_catch_error_message = "ERROR: Something went wrong with the service list.";
        public const string service_list_error_message = "ERROR: Service types not found.";
        public const string sidebarmenu_list_error_message = "ERROR: No sidebar list found.";
        public const string sidebarmenu_menuidisnull_error_message = "ERROR: Invalid menu ID.";
        public const string sidebarmenu_list_catch_error_message = "ERROR: Something went wrong with the sidebar menu list.";

        public const string sidebarmenu_list_success_message = "Something went wrong.";
        public const string service_list_success_message = "SUCCESS: Successfully.";
        public const string supscription_list_error_message = "ERROR: Something went wrong.";
        public const string supscription_list_success_message = "SUCCESS: Successfully.";
        public const string supscription_planlist_error_message = "ERROR: Plan type list not found.";
        public const string supscription_getlist_error_message = "ERROR: Subscription plan type not found.";

        public const string getmasterinfo_successfully = "SUCCESS: Successfully retrieved master info.";
        public const string getmasterinfo_invalidMenuIdandServiceTypeId = "ERROR: Invalid MenuId or ServiceTypeId.";
        public const string invalidApiKeyOrToken = "ERROR: Invalid ApiKey or Token!";
        public const string invalidPublicApiKey = "ERROR: Invalid Public Api Token!";
        public const string order_image_detail_null_arguments = "ERROR: Order image detail ID cannot be null!";
        public const string order_image_detail_fetching_null = "ERROR: This image cannot be found or others!";
        public const string order_image_detail_no_file_count = "ERROR: No order image detail list found.";
        public const string order_charge_break_down_order_master_id_null = "ERROR: Order master ID is null!";
        public const string getmasterinfo_orderIdNotFound = "ERROR: Order insert failed!";
        public const string question_answer_error_message = "ERROR: Something went wrong.";
        public const string question_answer_list_error_message = "No data available.";
        public const string question_answer_success_message = "SUCCESS: Successfully.";

        public const string servicetype_get_all_error_message = "No data available!";
        public const string package_cannot_found = "No package available by provided package id";
        public const string user_profile_can_not_update = "ERROR: User profile cannot be updated successfully!";
        public const string user_id_null = "ERROR: Cannot provide null user ID.";

        public const string setuserpassword_invaliduserIdOrPassword = "ERROR: Invalid UserID or Password.";
        public const string setuserpassword_passwordandconfirmpasswordnotsame = "ERROR: Password and Confirm Password do not match.";
        public const string account_user_already_verified_user = "ERROR: Account is already verified for this user.";
        public const string account_user_invalid_verification_token = "ERROR: Invalid verification token.";
        public const string setuserpassword_successfully_updated = "SUCCESS: User information updated successfully.";
        public const string setuserpassword_failed_updated = "ERROR: Failed to update user information.";
        public const string validate_token_try_catch_block = "ERROR: Something went wrong.";
        public const string setuserpassword_invaliduserid = "ERROR: Invalid UserID.";
        public const string validate_token_successfully = "SUCCESS: Token successfully validated.";
        public const string validate_token_null = "ERROR: You did not provide a token.";
        public const string resetuserpassword_invalidemail = "ERROR: Invalid email.";
        public const string resetuserpassword_success_message = "SUCCESS: Please check your email.";

        public const string insertorderimagedetail_invaliddata = "ERROR: Invalid data.";
        public const string order_image_service_failed_save = "ERROR: Update failed.";
        public const string order_image_detail_update_data_model = "ERROR: Invalid data.";
        public const string order_image_detail_try_catch_exception = "ERROR: Something went wrong in the try-catch block.";
        public const string order_master_info_model_validation = "ERROR: Invalid data.";
        public const string order_master_info_user_id_null = "ERROR: UserID is null or invalid.";
        public const string order_master_info_id_null = "ERROR: Order master image ID is null.";
        public const string order_master_info_try_catch = "ERROR: Failed with try-catch.";
        public const string order_master_info_passing_id_null = "ERROR: Order master ID is required.";
        public const string common_null_message = "ERROR: Someone ID is null.";
        public const string parameter_common_null_message = "ERROR: Parameter is required.";
        public const string common_success_message = "SUCCESS: Successfully retrieved all information.";
        public const string order_master_info_passing_id_is_not_correct = "ERROR: ID is not valid.";
        public const string insertorderimagedetail_successfully = "SUCCESS: Order image detail added successfully!";
        public const string order_image_service_successfull = "SUCCESS: Data updated successfully.";
        public const string order_image_service_fetching_successfull = "SUCCESS: Data fetching successful.";


        public const string order_image_detail_update_successfully = "SUCCESS: Order image detail updated successfully!";
        public const string auto_process_update_successfully = "SUCCESS: Successfully updated all.";
        public const string auto_process_update_failed = "ERROR: Failed to update all.";
        public const string order_image_detail_update_failed = "ERROR: Failed to update order image detail!";
        public const string order_master_info_update_successfully = "SUCCESS: Order master info updated successfully!";
        public const string insertorderimagedetail_falied = "ERROR: Failed to add order image detail!";
        public const string order_image_service_try_catch_failed = "ERROR: Something went wrong. See try-catch block for details!";
        public const string order_image_service_null = "ERROR: You have entered null data. Please provide the order image service ID.";
        public const string order_master_info_update_failed = "ERROR: Failed to update order master info!";
        public const string order_master_info_update_failed_from_json = "ERROR: Please ensure that all required fields are added.";
        public const string order_master_info_update_failed_try_catch = "ERROR: Something went wrong with the try-catch block!";
        public const string order_master_info_update_success_from_json = "SUCCESS: Successfully updated or inserted.";
        public const string order_master_info_update_failed_for_update = "ERROR: Internal processing error.";
        public const string order_master_info_fetching = "ERROR: Order master info cannot be found by the order ID.";
        public const string order_master_info_not_found = "ERROR: Order master info cannot be found.";
        public const string order_image_master_id_is_null = "ERROR: Order master ID cannot be found.";
        public const string auto_process_fetching = "ERROR: Auto process info cannot be found.";
        public const string order_placed_message = "ERROR: This order is already confirmed.";
        public const string order_master_count_zero = "ERROR: You do not have any pending orders.";
        public const string order_master_info_get_all_data_successfully = "SUCCESS: Successfully retrieved all data.";
        public const string auto_process_successfully_message = "SUCCESS: Successfully retrieved all data.";
        public const string user_promotion_insert_success_message = "SUCCESS: Data inserted successfully.";
        public const string user_promotion_insert_fail_message = "ERROR: Failed to insert data.";
        public const string user_default_setting_update_success_message = "SUCCESS: Successfully updated user default settings.";
        public const string user_default_setting_update_failed_message = "ERROR: Failed to update user default settings.";

        public const string user_not_found = "User not found";
        public const string user_token_not_verified = "Please Login Your Account!";
        public const string user_already_exist = "User already exists. Please try another email.";

        public const string user_or_public_key_not_match = "User or public key not match";
        public const string server_configuration_No_data_available = "No server configuration data available.";
        public const string server_configuration_successfully_message = "Successfully server configuation data getting.";
        public const string server_configuration_user_message = "Something wrong please try again.";
        public const string user_subscribe_successfully = "Successfully subscribe";

        public const string get_user_point_history_message = "SUCCESS: Successfully retrieved all data.";
        public const string get_user_point_history_failed = "Failed: Failed retrieved all data.";

        public const string update_user_spendpoint_successmessage = "SUCCESS: Successfully Update Spend Point.";
        public const string update_user_spendpoint_failedmessage = "Failed: Failed To Update Spend Point.";

        public const string get_ordermasterdetails_downloadablelink_successmessage = "SUCCESS: Successfully Get All Data.";
        public const string get_ordermasterdetails_downloadablelink_failedmessage = "Failed: Failed To Get All Data.";

        public const string user_pointmanage_failedmessage = "Failed: Internal Error on User point Manager.";

        public const string get_sub_plan_types_failed = "Failed: Failed retrieved all data.";
        public const string user_shopify_app_uninstall = "App uninstall successfully.";

        public const string purchase_package_empty_package_id = "Failed: No Package Id exist";
        public const string purchase_package_invalid_package_id = "Failed: Invalid Package Id";

        public const string order_status_update_failed = "failed: Order Status Update failed!";
        public const string order_status_update_success = "Success: Order Status Update Success";

        public const string order_confirmation_mail_send_failed = "Unable to sent Mail !";
        public const string order_confirmation_mail_send_sucess = "Mail Successfully Sent !";
        public const string order_file_upload_path_invalid = "Path is not valid !";
        public const string common_insert_success_message = "Data inserted successfully !";
        public const string common_inserted_failed_message = "Data is not inserted successfully !";
        public const string order_image_detail_count_null = "There is no data for this order master id !";
        public const string user_agent_null_exception = "User Agent Can not be null !";

        public const string common_no_data_available = "No data available.";
        public const string common_security_access_failed = "Public key or user not found.";

        public const string common_get_successfully = "Data get Sucessfully.";
        public const string common_get_failed = "Data not available";



    }
    public static class StatusResponseMessage
    {
        public const string failed = "failed";
        public const string success = "success";
    }

}
