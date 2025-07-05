// Mobile sidebar functionality
document.addEventListener('DOMContentLoaded', function() {
    const menuButton = document.querySelector('.menu-toggle');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');

    function checkWindowSize() {
        if (window.innerWidth <= 768) {
            sidebar.style.transform = 'translateX(-100%)';
            mainContent.style.marginLeft = '0';
            menuButton.style.display = 'flex';
        } else {
            sidebar.style.transform = 'translateX(0)';
            mainContent.style.marginLeft = '280px';
            menuButton.style.display = 'none';
        }
    }

    // Initial check
    checkWindowSize();

    // Event listener for window resize
    window.addEventListener('resize', checkWindowSize);

    // Event listener for menu button click
    menuButton.addEventListener('click', function() {
        if (sidebar.style.transform === 'translateX(0px)') {
            sidebar.style.transform = 'translateX(-100%)';
        } else {
            sidebar.style.transform = 'translateX(0)';
        }
    });

    // Modal functionality
    const modals = document.querySelectorAll('.modal');
    const closeButtons = document.querySelectorAll('.close-modal');
    
    // Function to open a specific modal
    function openModal(modalId) {
        document.getElementById(modalId).style.display = 'flex';
    }
    
    // Function to close all modals
    function closeAllModals() {
        modals.forEach(modal => {
            modal.style.display = 'none';
        });
    }
    
    // Add event listeners to close buttons
    closeButtons.forEach(button => {
        button.addEventListener('click', closeAllModals);
    });
    
    // Close modal when clicking outside
    window.addEventListener('click', function(event) {
        modals.forEach(modal => {
            if (event.target === modal) {
                modal.style.display = 'none';
            }
        });
    });

    // Show/hide limited operation details based on status check
    document.querySelectorAll('input[name="statusCheck"]').forEach(radio => {
        radio.addEventListener('change', function() {
            const limitedOperationDetails = document.getElementById('limitedOperationDetails');
            if (this.value === 'limitedOperation') {
                limitedOperationDetails.style.display = 'block';
            } else {
                limitedOperationDetails.style.display = 'none';
            }
        });
    });

    // Complete Maintenance Button Click
    const completeButtons = document.querySelectorAll('.complete-maintenance');
    completeButtons.forEach(button => {
        button.addEventListener('click', function() {
            const machineId = this.getAttribute('data-id');
            document.getElementById('completeMachineId').value = machineId;
            
            // Set default next maintenance date (30 days from now)
            const today = new Date();
            const nextMonth = new Date(today.setDate(today.getDate() + 30));
            document.getElementById('nextMaintenanceDate').valueAsDate = nextMonth;
            
            openModal('completeMaintenanceModal');
        });
    });

    // Extend Maintenance Button Click
    const extendButtons = document.querySelectorAll('.extend-maintenance');
    extendButtons.forEach(button => {
        button.addEventListener('click', function() {
            const machineId = this.getAttribute('data-id');
            document.getElementById('extendMachineId').value = machineId;
            
            // Set default estimated completion date (7 days from now)
            const today = new Date();
            const nextWeek = new Date(today.setDate(today.getDate() + 7));
            document.getElementById('newEstimatedCompletion').valueAsDate = nextWeek;
            
            openModal('extendMaintenanceModal');
        });
    });

    // Order Parts Button Click
    const orderPartsButtons = document.querySelectorAll('.order-parts');
    orderPartsButtons.forEach(button => {
        button.addEventListener('click', function() {
            const machineId = this.getAttribute('data-id');
            document.getElementById('orderPartsMachineId').value = machineId;
            openModal('orderPartsModal');
        });
    });

    // Add Log Button Click
    const addLogButtons = document.querySelectorAll('.add-maintenance-log');
    addLogButtons.forEach(button => {
        button.addEventListener('click', function() {
            const machineId = this.getAttribute('data-id');
            document.getElementById('logMachineId').value = machineId;
            openModal('addLogModal');
        });
    });

    // Add Log from View Button Click
    document.getElementById('addLogFromView').addEventListener('click', function() {
        const machineId = this.getAttribute('data-id');
        document.getElementById('logMachineId').value = machineId;
        closeAllModals();
        openModal('addLogModal');
    });

    // View Logs Button Click
    const viewLogsButtons = document.querySelectorAll('.view-maintenance-logs, .view-all-logs');
    viewLogsButtons.forEach(button => {
        button.addEventListener('click', function() {
            const machineId = this.getAttribute('data-id');
            const machineCard = document.querySelector(`.maintenance-card[data-machine-id="${machineId}"]`);
            const machineName = machineCard.querySelector('h3').textContent;
            
            document.getElementById('logMachineName').textContent = machineName;
            document.getElementById('logMachineId').textContent = `ID: #${machineId}`;
            document.getElementById('addLogFromView').setAttribute('data-id', machineId);
            
            // In a real app, you'd fetch all logs from the server
            // For now, we'll use the logData variable that's populated by the page
            const logsContainer = document.getElementById('maintenanceLogsContainer');
            
            // Get the logs for this machine from the global logData variable
            const machineLogs = logData[machineId] || [];
            
            if (machineLogs.length > 0) {
                let logsHtml = '<ul class="detailed-logs-list">';
                
                machineLogs.forEach(log => {
                    logsHtml += `
                        <li class="detailed-log-item">
                            <div class="log-timestamp">${log.date}</div>
                            <div class="log-content">${log.action}</div>
                        </li>
                    `;
                });
                
                logsHtml += '</ul>';
                logsContainer.innerHTML = logsHtml;
            } else {
                logsContainer.innerHTML = '<p class="no-logs">No maintenance logs available for this machine.</p>';
            }
            
            openModal('viewLogsModal');
        });
    });

    // Submit Complete Maintenance Form
    document.getElementById('submitCompleteMaintenance').addEventListener('click', function() {
        const form = document.getElementById('completeMaintenanceForm');
        const machineId = document.getElementById('completeMachineId').value;
        const workDone = document.getElementById('workDone').value;
        const notes = document.getElementById('maintenanceNotes').value;
        const limitedOperation = document.querySelector('input[name="statusCheck"]:checked').value === 'limitedOperation';
        const limitations = limitedOperation ? document.getElementById('operationLimitations').value : '';
        
        if (!workDone || !notes) {
            alert('Please fill in all required fields.');
            return;
        }
        
        // In a real app, you'd send an AJAX request to update the status
        // For now, we'll just show an alert
        alert('Maintenance completed for Machine #' + machineId);
        
        // In a real app, you'd reload the page
        // For demo purposes, we'll just hide the card
        const machineCard = document.querySelector(`.maintenance-card[data-machine-id="${machineId}"]`);
        machineCard.style.display = 'none';
        
        // Update the maintenance count
        const maintenanceCount = document.querySelector('.stat-value');
        maintenanceCount.textContent = parseInt(maintenanceCount.textContent) - 1;
        
        // Show "No machines under maintenance" if there are none left
        if (parseInt(maintenanceCount.textContent) === 0) {
            document.querySelector('.maintenance-cards').style.display = 'none';
            const noMaintenance = document.createElement('div');
            noMaintenance.className = 'no-maintenance';
            noMaintenance.innerHTML = `
                <div class="no-data-message">
                    <i class="fas fa-check-circle"></i>
                    <h3>No Machines Under Maintenance</h3>
                    <p>All machines are currently operational. Check the Machines page to view all machines.</p>
                    <a href="/Machines" class="btn btn-primary">
                        <i class="fas fa-cogs"></i>
                        View All Machines
                    </a>
                </div>
            `;
            document.querySelector('.maintenance-overview').insertAdjacentElement('afterend', noMaintenance);
        }
        
        // Reset and close the form
        form.reset();
        closeAllModals();
    });

    // Submit Extend Maintenance Form
    document.getElementById('submitExtendMaintenance').addEventListener('click', function() {
        const form = document.getElementById('extendMaintenanceForm');
        const machineId = document.getElementById('extendMachineId').value;
        const reason = document.getElementById('extensionReason').value;
        const details = document.getElementById('extensionDetails').value;
        const newDate = document.getElementById('newEstimatedCompletion').value;
        
        if (!reason || !details || !newDate) {
            alert('Please fill in all required fields.');
            return;
        }
        
        // In a real app, you'd send an AJAX request to update the maintenance
        // For now, we'll just show an alert
        alert('Maintenance extended for Machine #' + machineId);
        
        // Add the new log to the machine card
        const machineCard = document.querySelector(`.maintenance-card[data-machine-id="${machineId}"]`);
        const logsList = machineCard.querySelector('.logs-list');
        
        if (logsList) {
            // Create a new log entry
            const now = new Date();
            const formattedDate = `${now.getMonth() + 1}/${now.getDate()}/${now.getFullYear()} ${now.getHours()}:${now.getMinutes().toString().padStart(2, '0')}`;
            
            const newLog = document.createElement('li');
            newLog.className = 'log-item';
            newLog.innerHTML = `
                <span class="log-date">${formattedDate}</span>
                <span class="log-action">Maintenance extended: ${reason}. New estimated completion: ${new Date(newDate).toLocaleDateString()}</span>
            `;
            
            // Add the new log at the top
            if (logsList.firstChild) {
                logsList.insertBefore(newLog, logsList.firstChild);
            } else {
                logsList.appendChild(newLog);
            }
        }
        
        // Reset and close the form
        form.reset();
        closeAllModals();
    });

    // Submit Order Parts Form
    document.getElementById('submitOrderParts').addEventListener('click', function() {
        const form = document.getElementById('orderPartsForm');
        const machineId = document.getElementById('orderPartsMachineId').value;
        const partName = document.getElementById('partName').value;
        const quantity = document.getElementById('partQuantity').value;
        const urgency = document.getElementById('partUrgency').value;
        
        if (!partName || !quantity) {
            alert('Please fill in all required fields.');
            return;
        }
        
        // In a real app, you'd send an AJAX request to order the parts
        // For now, we'll just show an alert
        alert(`Parts ordered for Machine #${machineId}: ${partName} (${quantity})`);
        
        // Add the new log to the machine card
        const machineCard = document.querySelector(`.maintenance-card[data-machine-id="${machineId}"]`);
        const logsList = machineCard.querySelector('.logs-list');
        
        if (logsList) {
            // Create a new log entry
            const now = new Date();
            const formattedDate = `${now.getMonth() + 1}/${now.getDate()}/${now.getFullYear()} ${now.getHours()}:${now.getMinutes().toString().padStart(2, '0')}`;
            
            const newLog = document.createElement('li');
            newLog.className = 'log-item';
            newLog.innerHTML = `
                <span class="log-date">${formattedDate}</span>
                <span class="log-action">Parts ordered: ${partName} (Qty: ${quantity}). Urgency: ${urgency}</span>
            `;
            
            // Add the new log at the top
            if (logsList.firstChild) {
                logsList.insertBefore(newLog, logsList.firstChild);
            } else {
                logsList.appendChild(newLog);
            }
        }
        
        // Reset and close the form
        form.reset();
        closeAllModals();
    });

    // Submit Add Log Form
    document.getElementById('submitAddLog').addEventListener('click', function() {
        const form = document.getElementById('addLogForm');
        const machineId = document.getElementById('logMachineId').value;
        const logType = document.getElementById('logType').value;
        const logDetails = document.getElementById('logDetails').value;
        
        if (!logType || !logDetails) {
            alert('Please fill in all required fields.');
            return;
        }
        
        // In a real app, you'd send an AJAX request to add the log
        // For now, we'll just show an alert
        alert(`Log added to Machine #${machineId}`);
        
        // Add the new log to the machine card
        const machineCard = document.querySelector(`.maintenance-card[data-machine-id="${machineId}"]`);
        const logsList = machineCard.querySelector('.logs-list');
        
        if (logsList) {
            // Create a new log entry
            const now = new Date();
            const formattedDate = `${now.getMonth() + 1}/${now.getDate()}/${now.getFullYear()} ${now.getHours()}:${now.getMinutes().toString().padStart(2, '0')}`;
            
            const newLog = document.createElement('li');
            newLog.className = 'log-item';
            newLog.innerHTML = `
                <span class="log-date">${formattedDate}</span>
                <span class="log-action">${logType}: ${logDetails}</span>
            `;
            
            // Add the new log at the top
            if (logsList.firstChild) {
                logsList.insertBefore(newLog, logsList.firstChild);
            } else {
                logsList.appendChild(newLog);
            }
        }
        
        // Reset and close the form
        form.reset();
        closeAllModals();
    });

    // Refresh button
    document.getElementById('refreshMaintenance').addEventListener('click', function() {
        // In a real app, you'd reload the data from the server
        // For now, we'll just show an alert
        alert('Refreshing maintenance data...');
        // In a real app, you'd make an AJAX call here
    });
});