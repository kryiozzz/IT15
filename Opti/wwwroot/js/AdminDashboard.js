// This code should replace the initializeCharts function in your AdminDashboard.js

// Store chart instances globally to prevent recreation
let salesChart = null;
let customersChart = null;
let machinesChart = null;

// Initialize all charts
function initializeCharts() {
    console.log("Initializing charts - only one time");

    // Only initialize if charts don't already exist
    if (salesChart || customersChart || machinesChart) {
        console.log("Charts already initialized, skipping");
        return;
    }

    const salesData = {
        labels: monthNames,
        datasets: [{
            label: 'Sales ($)',
            data: monthlySales,
            backgroundColor: 'rgba(59, 130, 246, 0.2)',
            borderColor: 'rgba(59, 130, 246, 1)',
            borderWidth: 2,
            tension: 0.4,
            fill: true,
            pointBackgroundColor: 'rgba(59, 130, 246, 1)',
        }]
    };

    const customersData = {
        labels: monthNames,
        datasets: [{
            label: 'New Customers',
            data: monthlyCustomerGrowth,
            backgroundColor: 'rgba(16, 185, 129, 0.6)',
            borderColor: 'rgba(16, 185, 129, 1)',
            borderWidth: 1,
            borderRadius: 6,
            maxBarThickness: 45
        }]
    };

    const machinesData = {
        labels: ['Operational', 'Under Maintenance', 'Offline'],
        datasets: [{
            label: 'Machines',
            data: [operationalMachines, machinesUnderMaintenance, offlineMachines],
            backgroundColor: [
                'rgba(34, 197, 94, 0.7)',
                'rgba(245, 158, 11, 0.7)',
                'rgba(239, 68, 68, 0.7)'
            ],
            borderColor: [
                'rgba(34, 197, 94, 1)',
                'rgba(245, 158, 11, 1)',
                'rgba(239, 68, 68, 1)'
            ],
            borderWidth: 2,
            hoverOffset: 15
        }]
    };

    // Only initialize sales chart if the element exists and the chart doesn't already exist
    const salesCanvas = document.getElementById('salesChart');
    if (salesCanvas && !salesChart) {
        const salesCtx = salesCanvas.getContext('2d');
        salesChart = new Chart(salesCtx, {
            type: 'line',
            data: salesData,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                aspectRatio: 2,
                animation: {
                    duration: 0 // Disable animations to prevent refreshing appearance
                },
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(226, 232, 240, 0.5)'
                        },
                        ticks: {
                            callback: function (value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                }
            }
        });
    }

    // Only initialize customers chart if the element exists and the chart doesn't already exist
    const customersCanvas = document.getElementById('customersChart');
    if (customersCanvas && !customersChart) {
        const customersCtx = customersCanvas.getContext('2d');
        customersChart = new Chart(customersCtx, {
            type: 'bar',
            data: customersData,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                aspectRatio: 2,
                animation: {
                    duration: 0 // Disable animations to prevent refreshing appearance
                },
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(226, 232, 240, 0.5)'
                        },
                        ticks: {
                            precision: 0,
                            stepSize: 1
                        }
                    }
                }
            }
        });
    }

    // Only initialize machines chart if the element exists and the chart doesn't already exist
    const machinesCanvas = document.getElementById('machinesChart');
    if (machinesCanvas && !machinesChart) {
        const machinesCtx = machinesCanvas.getContext('2d');
        machinesChart = new Chart(machinesCtx, {
            type: 'doughnut',
            data: machinesData,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                aspectRatio: 2.5,
                animation: {
                    duration: 0 // Disable animations to prevent refreshing appearance
                },
                cutout: '70%',
                plugins: {
                    legend: {
                        position: 'right',
                        align: 'center',
                        labels: {
                            padding: 15,
                            usePointStyle: true,
                            pointStyle: 'circle'
                        }
                    }
                }
            }
        });
    }
}

// Initialize event listeners
function initializeEventListeners() {
    // Avoid multiple event listener registrations
    document.removeEventListener('mousemove', handleUserActivity);
    document.removeEventListener('keydown', handleUserActivity);

    // Add single event listeners
    document.addEventListener('mousemove', handleUserActivity);
    document.addEventListener('keydown', handleUserActivity);

    // Rest of your event listener initialization...
}

// Update existing charts instead of recreating them
function updateCharts() {
    if (salesChart) {
        salesChart.data.labels = monthNames;
        salesChart.data.datasets[0].data = monthlySales;
        salesChart.update({ duration: 0 });
    }

    if (customersChart) {
        customersChart.data.labels = monthNames;
        customersChart.data.datasets[0].data = monthlyCustomerGrowth;
        customersChart.update({ duration: 0 });
    }

    if (machinesChart) {
        machinesChart.data.datasets[0].data = [operationalMachines, machinesUnderMaintenance, offlineMachines];
        machinesChart.update({ duration: 0 });
    }
}

// Initialize dashboard only once
let dashboardInitialized = false;
document.addEventListener('DOMContentLoaded', function () {
    if (!dashboardInitialized) {
        initializeCharts();
        initializeEventListeners();
        startSessionTimer();
        console.log("Dashboard initialized once");
        dashboardInitialized = true;
    } else {
        console.log("Dashboard already initialized, skipping");
    }
});

// Clean up charts to prevent memory leaks when navigating away
window.addEventListener('beforeunload', function () {
    if (salesChart) salesChart.destroy();
    if (customersChart) customersChart.destroy();
    if (machinesChart) machinesChart.destroy();
    salesChart = null;
    customersChart = null;
    machinesChart = null;
});