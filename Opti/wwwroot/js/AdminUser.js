/**
 * AdminUsers.js
 * JavaScript functionality for the users management page
 */

// Variables
let currentUserId = 0;
let currentPage = 1;
let totalPages = 1;
let usersPerPage = 10;
let userData = [];
let filteredUsers = [];

// Document ready
$(document).ready(function () {
    // Add event listeners
    $('#addUserBtn, #addNewUserBtn').click(openAddUserModal);
    $('#userForm').submit(saveUser);
    $('#userSearchInput').on('input', searchUsers);
    $('#exportUsersBtn').click(exportUsers);
    $('#prevPage').click(goToPrevPage);
    $('#nextPage').click(goToNextPage);

    // Initialize the page
    initializeUserData();
    updatePagination();
});

/**
 * Initialize user data from the table
 */
function initializeUserData() {
    userData = [];

    // Extract user data from the table rows
    $('#usersTable tbody tr').each(function () {
        const cells = $(this).find('td');
        if (cells.length < 8) return; // Skip rows without enough cells (like "No users found")

        const userId = parseInt(cells.eq(0).text());
        const username = cells.eq(1).text();
        const email = cells.eq(2).text();
        const roleElement = cells.eq(3).find('.badge');
        const role = roleElement.text().trim();
        const createdAt = cells.eq(4).text();
        const lastLoginDate = cells.eq(5).text() !== 'Never' ? cells.eq(5).text() : null;
        const statusElement = cells.eq(6).find('.badge');
        const isActive = statusElement.hasClass('badge-success');

        userData.push({
            userId,
            username,
            email,
            role,
            createdAt,
            lastLoginDate,
            isActive
        });
    });

    // Initialize filtered users
    filteredUsers = [...userData];
}

/**
 * Open modal for adding a new user
 */
function openAddUserModal() {
    resetUserForm();
    $('#userModalTitle').text('Add New User');
    $('#passwordGroup, #confirmPasswordGroup').show();
    $('#userForm').attr('data-mode', 'add');
    $('#isActive').prop('checked', true); // Default to active
    $('#userFormModal').css('display', 'flex');
}

/**
 * Open modal for editing an existing user
 * @param {number} userId - ID of the user to edit
 */
function editUser(userId) {
    // Find user data
    const user = findUserById(userId);
    if (!user) {
        showAlert('User not found', 'error');
        return;
    }

    // Populate the form
    resetUserForm();
    $('#userId').val(userId);
    $('#username').val(user.username);
    $('#email').val(user.email);
    $('#role').val(user.role);
    $('#isActive').prop('checked', user.isActive);

    // Hide password fields for edit
    $('#passwordGroup, #confirmPasswordGroup').hide();

    // Set modal title and mode
    $('#userModalTitle').text('Edit User');
    $('#userForm').attr('data-mode', 'edit');

    // Show the modal
    $('#userFormModal').css('display', 'flex');
}

/**
 * Open modal for viewing user details
 * @param {number} userId - ID of the user to view
 */
function viewUser(userId) {
    // Find user data
    const user = findUserById(userId);
    if (!user) {
        showAlert('User not found', 'error');
        return;
    }

    // Populate the view modal
    $('#userInitials').text(getInitials(user.username));
    $('#viewUsername').text(user.username);
    $('#viewUserRole').text(user.role);
    $('#viewUserEmail').text(user.email);
    $('#viewUserId').text(user.userId);
    $('#viewUserCreated').text(user.createdAt);
    $('#viewUserLastLogin').text(user.lastLoginDate || 'Never');

    // Set status badge
    const statusBadge = user.isActive ?
        '<span class="badge badge-success">Active</span>' :
        '<span class="badge badge-danger">Inactive</span>';
    $('#viewUserStatus').html(statusBadge);

    // Store the current user ID
    currentUserId = userId;

    // Show the modal
    $('#viewUserModal').css('display', 'flex');
}

/**
 * Edit user from view modal
 */
function editUserFromView() {
    closeModal('viewUserModal');
    editUser(currentUserId);
}

/**
 * Reset user form
 */
function resetUserForm() {
    $('#userForm')[0].reset();
    $('#userId').val(0);
}

/**
 * Toggle password visibility
 */
function togglePasswordVisibility() {
    const passwordInput = document.getElementById('password');
    const icon = document.querySelector('.password-toggle i');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}

/**
 * Close modal
 * @param {string} modalId - ID of the modal to close
 */
function closeModal(modalId) {
    document.getElementById(modalId).style.display = 'none';
}

/**
 * Save user (create or update)
 * @param {Event} e - Form submit event
 */
function saveUser(e) {
    e.preventDefault();

    // Validate form
    if (!validateUserForm()) return;

    // Get form data
    const mode = $('#userForm').attr('data-mode');
    const userId = parseInt($('#userId').val());
    const userData = {
        userId: userId,
        username: $('#username').val().trim(),
        email: $('#email').val().trim(),
        role: $('#role').val(),
        isActive: $('#isActive').is(':checked')
    };

    // Add password for new users
    if (mode === 'add') {
        userData.password = $('#password').val();
    }

    // In a real app, you would send this data to the server using AJAX
    // Here we're simulating a successful response

    // For demo purposes, simulate API call with timeout
    showAlert('Saving user...', 'info');

    setTimeout(() => {
        if (mode === 'add') {
            // Simulate adding a new user
            simulateAddUser(userData);
        } else {
            // Simulate updating an existing user
            simulateUpdateUser(userData);
        }

        // Close the modal
        closeModal('userFormModal');

        // Show success message
        showAlert(`User ${mode === 'add' ? 'created' : 'updated'} successfully!`, 'success');
    }, 1000);
}

/**
 * Validate user form
 * @returns {boolean} - True if valid, false otherwise
 */
function validateUserForm() {
    const mode = $('#userForm').attr('data-mode');
    const username = $('#username').val().trim();
    const email = $('#email').val().trim();
    const role = $('#role').val();

    // Check required fields
    if (!username || !email || !role) {
        showAlert('Please fill all required fields.', 'error');
        return false;
    }

    // Validate email format
    if (!isValidEmail(email)) {
        showAlert('Please enter a valid email address.', 'error');
        return false;
    }

    // Validate password for new users
    if (mode === 'add') {
        const password = $('#password').val();
        const confirmPassword = $('#confirmPassword').val();

        if (!password) {
            showAlert('Please enter a password.', 'error');
            return false;
        }

        if (password.length < 6) {
            showAlert('Password must be at least 6 characters long.', 'error');
            return false;
        }

        if (password !== confirmPassword) {
            showAlert('Passwords do not match.', 'error');
            return false;
        }
    }

    return true;
}

/**
 * Toggle user active status
 * @param {number} userId - ID of the user
 * @param {boolean} newStatus - New status (true for active, false for inactive)
 */
function toggleUserStatus(userId, newStatus) {
    // In a real app, you would update this on the server via AJAX
    // Here we're just updating the UI

    // Show loading message
    showAlert('Updating user status...', 'info');

    // Simulate API call
    setTimeout(() => {
        // Find the user in the array and update status
        const userIndex = userData.findIndex(user => user.userId === userId);
        if (userIndex !== -1) {
            userData[userIndex].isActive = newStatus;
            filteredUsers = [...userData]; // Reset filtered users

            if ($('#userSearchInput').val()) {
                searchUsers(); // Re-apply search if there's a search term
            }
        }

        // Find the user row in the table
        const row = $(`#usersTable tbody tr`).filter(function () {
            return $(this).find('td:first').text() == userId;
        });

        // Update the status badge
        const statusCell = row.find('td:nth-child(7)');
        const statusText = newStatus ? 'Active' : 'Inactive';
        const badgeClass = newStatus ? 'badge-success' : 'badge-danger';
        statusCell.html(`<span class="badge ${badgeClass}">${statusText}</span>`);

        // Update the action button
        const actionButton = row.find('td:last button:last');
        actionButton.attr('onclick', `toggleUserStatus(${userId}, ${!newStatus})`);
        actionButton.attr('title', `${newStatus ? 'Deactivate' : 'Activate'} User`);

        const iconClass = newStatus ? 'fa-user-slash' : 'fa-user-check';
        actionButton.find('i').attr('class', `fas ${iconClass}`);

        // Show success message
        showAlert(`User ${statusText.toLowerCase()} status updated successfully!`, 'success');
    }, 1000);
}

/**
 * Search users based on input
 */
function searchUsers() {
    const searchTerm = $('#userSearchInput').val().toLowerCase();

    if (!searchTerm) {
        // If search term is empty, show all users
        filteredUsers = [...userData];
    } else {
        // Filter users based on search term
        filteredUsers = userData.filter(user =>
            user.username.toLowerCase().includes(searchTerm) ||
            user.email.toLowerCase().includes(searchTerm) ||
            user.role.toLowerCase().includes(searchTerm)
        );
    }

    // Reset to first page and update the table
    currentPage = 1;
    updateUserTable();
    updatePagination();
}

/**
 * Export users to CSV
 */
function exportUsers() {
    // Get data to export
    const dataToExport = filteredUsers.length > 0 ? filteredUsers : userData;

    // Convert to CSV
    let csvContent = "data:text/csv;charset=utf-8,";

    // Add headers
    csvContent += "ID,Username,Email,Role,Created,Last Login,Status\n";

    // Add data rows
    dataToExport.forEach(user => {
        csvContent += `${user.userId},"${user.username}","${user.email}","${user.role}","${user.createdAt}","${user.lastLoginDate || 'Never'}","${user.isActive ? 'Active' : 'Inactive'}"\n`;
    });

    // Create download link
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", "users_export.csv");
    document.body.appendChild(link);

    // Trigger download
    link.click();

    // Clean up
    document.body.removeChild(link);

    // Show success message
    showAlert(`Exported ${dataToExport.length} users to CSV`, 'success');
}

/**
 * Update pagination based on current data
 */
function updatePagination() {
    const totalUsers = filteredUsers.length;
    totalPages = Math.ceil(totalUsers / usersPerPage);

    if (totalPages === 0) totalPages = 1;

    // Update page info text
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);

    // Enable/disable buttons
    $('#prevPage').prop('disabled', currentPage === 1);
    $('#nextPage').prop('disabled', currentPage === totalPages);

    // Update the table
    updateUserTable();
}

/**
 * Go to previous page
 */
function goToPrevPage() {
    if (currentPage > 1) {
        currentPage--;
        updateUserTable();
        updatePagination();
    }
}

/**
 * Go to next page
 */
function goToNextPage() {
    if (currentPage < totalPages) {
        currentPage++;
        updateUserTable();
        updatePagination();
    }
}

/**
 * Update user table with current page data
 */
function updateUserTable() {
    const startIndex = (currentPage - 1) * usersPerPage;
    const endIndex = startIndex + usersPerPage;
    const usersToShow = filteredUsers.slice(startIndex, endIndex);

    // Clear table
    const tableBody = $('#usersTable tbody');
    tableBody.empty();

    // If no users to show
    if (usersToShow.length === 0) {
        tableBody.html('<tr><td colspan="8" class="text-center py-4">No users found</td></tr>');
        return;
    }

    // Add rows for current page users
    usersToShow.forEach(user => {
        const roleBadgeClass = user.role === 'Admin' ? 'badge-info' : 'badge-success';
        const statusBadgeClass = user.isActive ? 'badge-success' : 'badge-danger';
        const actionIconClass = user.isActive ? 'fa-user-slash' : 'fa-user-check';
        const actionTitle = user.isActive ? 'Deactivate' : 'Activate';

        const row = `
            <tr>
                <td>${user.userId}</td>
                <td>${user.username}</td>
                <td>${user.email}</td>
                <td><span class="badge ${roleBadgeClass}">${user.role}</span></td>
                <td>${user.createdAt}</td>
                <td>${user.lastLoginDate || 'Never'}</td>
                <td><span class="badge ${statusBadgeClass}">${user.isActive ? 'Active' : 'Inactive'}</span></td>
                <td>
                    <div class="flex gap-2">
                        <button class="btn-icon" onclick="viewUser(${user.userId})" title="View User">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn-icon" onclick="editUser(${user.userId})" title="Edit User">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn-icon text-danger" onclick="toggleUserStatus(${user.userId}, ${!user.isActive})" title="${actionTitle} User">
                            <i class="fas ${actionIconClass}"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;

        tableBody.append(row);
    });
}

/**
 * Simulate adding a new user (in a real app this would be an API call)
 * @param {Object} userData - User data
 */
function simulateAddUser(userData) {
    // Generate a new ID (in a real app this would come from the server)
    const newId = Math.max(...this.userData.map(user => user.userId), 0) + 1;

    // Create new user object
    const newUser = {
        userId: newId,
        username: userData.username,
        email: userData.email,
        role: userData.role,
        createdAt: new Date().toLocaleDateString(),
        lastLoginDate: null,
        isActive: userData.isActive
    };

    // Add to the array
    this.userData.unshift(newUser);
    filteredUsers = [...this.userData];

    // Re-apply search if needed
    if ($('#userSearchInput').val()) {
        searchUsers();
    } else {
        updateUserTable();
        updatePagination();
    }
}

/**
 * Simulate updating an existing user (in a real app this would be an API call)
 * @param {Object} userData - User data
 */
function simulateUpdateUser(userData) {
    // Find the user in the array
    const userIndex = this.userData.findIndex(user => user.userId === userData.userId);

    if (userIndex !== -1) {
        // Update user data
        this.userData[userIndex].username = userData.username;
        this.userData[userIndex].email = userData.email;
        this.userData[userIndex].role = userData.role;
        this.userData[userIndex].isActive = userData.isActive;

        // Update filtered users
        filteredUsers = [...this.userData];

        // Re-apply search if needed
        if ($('#userSearchInput').val()) {
            searchUsers();
        } else {
            updateUserTable();
        }
    }
}

/**
 * Find user by ID
 * @param {number} userId - User ID
 * @returns {Object|null} - User object or null if not found
 */
function findUserById(userId) {
    return userData.find(user => user.userId === parseInt(userId)) || null;
}

/**
 * Show alert message
 * @param {string} message - Message to show
 * @param {string} type - Alert type (success, error, warning, info)
 */
function showAlert(message, type = 'info') {
    const alertContainer = $('#alertContainer');
    const alertId = Date.now();
    const alertClass = type === 'error' ? 'alert-danger' :
        type === 'success' ? 'alert-success' :
            type === 'warning' ? 'alert-warning' : 'alert-info';

    const alertHtml = `
        <div class="alert ${alertClass}" id="alert-${alertId}">
            <div class="alert-content">
                <i class="alert-icon fas ${type === 'error' ? 'fa-exclamation-circle' :
            type === 'success' ? 'fa-check-circle' :
                type === 'warning' ? 'fa-exclamation-triangle' : 'fa-info-circle'}"></i>
                <div class="alert-message">${message}</div>
            </div>
            <button class="alert-close" onclick="dismissAlert('alert-${alertId}')">&times;</button>
        </div>
    `;

    alertContainer.append(alertHtml);

    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        dismissAlert(`alert-${alertId}`);
    }, 5000);
}

/**
 * Dismiss alert
 * @param {string} alertId - ID of alert to dismiss
 */
function dismissAlert(alertId) {
    $(`#${alertId}`).fadeOut(300, function () {
        $(this).remove();
    });
}

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} - True if valid, false otherwise
 */
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Get initials from name
 * @param {string} name - Name to get initials from
 * @returns {string} - Initials
 */
function getInitials(name) {
    if (!name) return 'U';
    const nameParts = name.trim().split(' ');
    if (nameParts.length === 1) return nameParts[0].charAt(0).toUpperCase();
    return (nameParts[0].charAt(0) + nameParts[nameParts.length - 1].charAt(0)).toUpperCase();
}