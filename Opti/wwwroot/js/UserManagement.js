// User Management JavaScript
$(document).ready(function() {
    // Variables for pagination
    let currentPage = 1;
    const pageSize = 10;
    let allUsers = [];
    let filteredUsers = [];
    let activeFilters = {
        roles: ["Admin", "Worker", "Customer"],
        statuses: [true, false]
    };

    // Load users and stats on page load
    loadUsers();
    loadUserStats();

    // Toggle add user form
    $("#toggleAddUserForm").click(function() {
        $("#addUserForm").slideToggle();
        $(this).find('i').toggleClass('fa-plus fa-minus');
    });

    // Cancel add user
    $("#cancelAddUser").click(function() {
        $("#addUserForm")[0].reset();
        $("#addUserForm").slideUp();
        $("#toggleAddUserForm").find('i').removeClass('fa-minus').addClass('fa-plus');
    });

    // Add user form submit
    $("#addUserForm").submit(function(e) {
        e.preventDefault();

        // Validate password match
        if ($("#password").val() !== $("#confirmPassword").val()) {
            showAlert("Passwords do not match!", "error");
            return;
        }

        // Get form data
        const formData = {
            Username: $("#username").val(),
            Email: $("#email").val(),
            Password: $("#password").val(),
            Role: $("#role").val()
        };

        // Send data to server
        $.ajax({
            url: '/AdminDashboard/CreateUser',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    showAlert(response.message, "success");
                    $("#addUserForm")[0].reset();
                    $("#addUserForm").slideUp();
                    $("#toggleAddUserForm").find('i').removeClass('fa-minus').addClass('fa-plus');
                    loadUsers();
                    loadUserStats();
                } else {
                    showAlert(response.message, "error");
                }
            },
            error: function(xhr) {
                showAlert("An error occurred while creating the user.", "error");
                console.error(xhr.responseText);
            }
        });
    });

    // Toggle password reset fields
    $("#resetPassword").change(function() {
        $("#passwordResetFields").slideToggle(this.checked);
    });

    // ALTERNATIVE APPROACH: Use direct form submission instead of AJAX
    $("#editUserForm").off('submit').on('submit', function(e) {
        e.preventDefault();
        
        // Get form data as an object with correct property names
        const userId = parseInt($("#editUserId").val());
        const username = $("#editUsername").val();
        const email = $("#editEmail").val();
        const role = $("#editRole").val();
        const status = $("#editStatus").val();
        const resetPassword = $("#resetPassword").is(":checked");
        let newPassword = null;
        
        // Validate password match if resetting
        if (resetPassword) {
            if ($("#newPassword").val() !== $("#confirmNewPassword").val()) {
                showAlert("Passwords do not match!", "error");
                return;
            }
            newPassword = $("#newPassword").val();
        }
        
        // Create a hidden form to submit
        const hiddenForm = $('<form>').attr({
            method: 'POST',
            action: '/AdminDashboard/UpdateUserDirect'
        }).css('display', 'none');
        
        // Add CSRF token
        hiddenForm.append($('<input>').attr({
            type: 'hidden',
            name: '__RequestVerificationToken',
            value: $('input[name="__RequestVerificationToken"]').val()
        }));
        
        // Add form fields
        hiddenForm.append($('<input>').attr({
            type: 'hidden',
            name: 'UserId',
            value: userId
        }));
        
        hiddenForm.append($('<input>').attr({
            type: 'hidden',
            name: 'Username',
            value: username
        }));
        
        hiddenForm.append($('<input>').attr({
            type: 'hidden',
            name: 'Email',
            value: email
        }));
        
        hiddenForm.append($('<input>').attr({
            type: 'hidden',
            name: 'Role',
            value: role
        }));
        
        hiddenForm.append($('<input>').attr({
            type: 'hidden',
            name: 'Status',
            value: status
        }));
        
        hiddenForm.append($('<input>').attr({
            type: 'hidden',
            name: 'ResetPassword',
            value: resetPassword
        }));
        
        if (resetPassword && newPassword) {
            hiddenForm.append($('<input>').attr({
                type: 'hidden',
                name: 'NewPassword',
                value: newPassword
            }));
        }
        
        // Add form to body and submit
        $('body').append(hiddenForm);
        hiddenForm.submit();
    });

    // User search functionality
    $("#userSearch").on("keyup", function() {
        applyFiltersAndSearch();
    });

    // Toggle filter dropdown
    $("#filterButton").click(function(e) {
        e.stopPropagation();
        $("#filterDropdown").toggleClass("show");
    });

    // Close filter dropdown when clicking outside
    $(document).click(function(e) {
        if (!$(e.target).closest('.filter-dropdown').length) {
            $("#filterDropdown").removeClass("show");
        }
    });

    // Reset filters
    $("#resetFilters").click(function() {
        $(".role-filter, .status-filter").prop("checked", true);
    });

    // Apply filters
    $("#applyFilters").click(function() {
        // Update active filters
        activeFilters.roles = [];
        $(".role-filter:checked").each(function() {
            activeFilters.roles.push($(this).val());
        });

        activeFilters.statuses = [];
        $(".status-filter:checked").each(function() {
            activeFilters.statuses.push($(this).val() === "true");
        });

        // Apply filters
        applyFiltersAndSearch();
        
        // Close dropdown
        $("#filterDropdown").removeClass("show");
    });

    // Function to apply filters and search
    function applyFiltersAndSearch() {
        const searchTerm = $("#userSearch").val().toLowerCase();
        
        // Apply filters and search
        filteredUsers = allUsers.filter(user => 
            // Filter by role
            activeFilters.roles.includes(user.role) &&
            // Filter by status
            activeFilters.statuses.includes(user.isActive) &&
            // Filter by search term
            (searchTerm === "" || 
             user.username.toLowerCase().includes(searchTerm) ||
             user.email.toLowerCase().includes(searchTerm) ||
             user.role.toLowerCase().includes(searchTerm))
        );
        
        // Reset to first page
        currentPage = 1;
        
        // Display filtered users
        displayUsers(filteredUsers);
    }

    // Pagination buttons
    $("#prevPage").click(function() {
        if (currentPage > 1) {
            currentPage--;
            displayUsers(filteredUsers);
        }
    });

    $("#nextPage").click(function() {
        const totalPages = Math.ceil(filteredUsers.length / pageSize);
        if (currentPage < totalPages) {
            currentPage++;
            displayUsers(filteredUsers);
        }
    });

    // Load users from server
    function loadUsers() {
        $.ajax({
            url: '/AdminDashboard/GetUsers',
            type: 'GET',
            success: function(response) {
                if (response.error) {
                    showAlert(response.error, "error");
                    return;
                }

                // Store all users
                allUsers = response;
                filteredUsers = [...allUsers]; // Initialize filtered users

                // Apply filters (in case filters were set)
                applyFiltersAndSearch();
            },
            error: function(xhr) {
                showAlert("An error occurred while loading users.", "error");
                console.error(xhr.responseText);
            }
        });
    }

    // Load user statistics from server
    function loadUserStats() {
        $.ajax({
            url: '/AdminDashboard/GetUserStats',
            type: 'GET',
            success: function(response) {
                if (response.error) {
                    showAlert(response.error, "error");
                    return;
                }

                // Update stats
                $("#totalUsers").text(response.totalUsers);
                $("#adminUsers").text(response.adminUsers);
                $("#workerUsers").text(response.workerUsers);
                $("#customerUsers").text(response.customerUsers);
                $("#newUsers").text(response.newUsers);

                // Admin, worker, and customer percentages
                const adminPercentage = response.totalUsers > 0
                    ? ((response.adminUsers / response.totalUsers) * 100).toFixed(1)
                    : 0;
                const workerPercentage = response.totalUsers > 0
                    ? ((response.workerUsers / response.totalUsers) * 100).toFixed(1)
                    : 0;
                const customerPercentage = response.totalUsers > 0
                    ? ((response.customerUsers / response.totalUsers) * 100).toFixed(1)
                    : 0;

                $("#adminPercentage").text(`${adminPercentage}%`);
                $("#workerPercentage").text(`${workerPercentage}%`);
                $("#customerPercentage").text(`${customerPercentage}%`);

                // Growth rate
                $("#userGrowthRate").text(`${response.growthRate}%`);

                // Update stat-change class based on growth rate
                if (response.growthRate > 0) {
                    $("#totalUsers").closest(".stat-card").find(".stat-change")
                        .removeClass("stat-decrease").addClass("stat-increase")
                        .find("i").removeClass("fa-arrow-down").addClass("fa-arrow-up");
                } else if (response.growthRate < 0) {
                    $("#totalUsers").closest(".stat-card").find(".stat-change")
                        .removeClass("stat-increase").addClass("stat-decrease")
                        .find("i").removeClass("fa-arrow-up").addClass("fa-arrow-down");
                }
            },
            error: function(xhr) {
                showAlert("An error occurred while loading user statistics.", "error");
                console.error(xhr.responseText);
            }
        });
    }

    // Display users with pagination
    function displayUsers(users) {
        // Calculate pagination
        const totalPages = Math.ceil(users.length / pageSize);
        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = Math.min(startIndex + pageSize, users.length);
        const usersToDisplay = users.slice(startIndex, endIndex);

        // Update pagination buttons
        $("#prevPage").prop("disabled", currentPage === 1);
        $("#nextPage").prop("disabled", currentPage === totalPages || totalPages === 0);

        // Update user count
        $("#userCount").text(`${usersToDisplay.length} of ${users.length}`);

        // Clear table
        $("#usersTableBody").empty();

        // Add users to table
        if (usersToDisplay.length > 0) {
            usersToDisplay.forEach(user => {
                const row = `
                    <tr>
                        <td>${user.userId}</td>
                        <td>${user.username}</td>
                        <td>${user.email}</td>
                        <td>
                            <span class="user-role ${user.role === 'Admin' ? 'role-admin' : user.role === 'Worker' ? 'role-worker' : 'role-customer'}">
                                ${user.role}
                            </span>
                        </td>
                        <td>
                            <span class="user-status ${user.isActive ? 'status-active' : 'status-inactive'}"></span>
                            ${user.isActive ? 'Active' : 'Inactive'}
                        </td>
                        <td>${new Date(user.createdAt).toLocaleDateString()}</td>
                        <td>
                            <div class="user-actions">
                                <button class="btn-icon edit-user" data-id="${user.userId}">
                                    <i class="fas fa-edit"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;

                $("#usersTableBody").append(row);
            });

            // Add event listeners for edit buttons
            $(".edit-user").click(function() {
                const userId = $(this).data("id");
                openEditUserModal(userId);
            });
        } else {
            $("#usersTableBody").append(`
                <tr>
                    <td colspan="7" class="text-center py-4">No users found</td>
                </tr>
            `);
        }
    }

    // Open edit user modal
    function openEditUserModal(userId) {
        // Fetch user details from server
        $.ajax({
            url: `/AdminDashboard/GetUser?id=${userId}`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.user) {
                    const user = response.user;

                    // Populate form
                    $("#editUserId").val(user.userId);
                    $("#editUsername").val(user.username);
                    $("#editEmail").val(user.email);
                    $("#editRole").val(user.role);
                    $("#editStatus").val(user.isActive ? "Active" : "Inactive");

                    // Reset password checkbox
                    $("#resetPassword").prop("checked", false);
                    $("#passwordResetFields").hide();

                    // Open modal
                    openModal('editUserModal');
                } else {
                    showAlert(response.message || "Failed to load user details", "error");
                }
            },
            error: function(xhr) {
                showAlert("An error occurred while loading user details.", "error");
                console.error(xhr.responseText);
            }
        });
    }

    // Show alert message
    function showAlert(message, type) {
        const alertClass = type === "success" ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800";
        const icon = type === "success" ? "fas fa-check-circle" : "fas fa-exclamation-circle";

        const alert = `
            <div class="alert ${alertClass} rounded-lg p-4 mb-4 flex items-center shadow-sm">
                <i class="${icon} mr-2"></i>
                ${message}
                <button class="ml-auto close-alert" type="button">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;

        $("#alertContainer").append(alert);

        // Add event listener to close button
        $(".close-alert").click(function() {
            $(this).parent().fadeOut('fast', function() {
                $(this).remove();
            });
        });

        // Auto-remove after 5 seconds
        setTimeout(function() {
            $(".alert").first().fadeOut('fast', function() {
                $(this).remove();
            });
        }, 5000);
    }
});

// Modal functions
function openModal(modalId) {
    $(`#${modalId}`).addClass('active');
    $('body').addClass('modal-open');
}

function closeModal(modalId) {
    $(`#${modalId}`).removeClass('active');
    $('body').removeClass('modal-open');
}