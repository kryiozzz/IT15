document.addEventListener('DOMContentLoaded', function() {
    // Parallax effect for hero section
    const heroSection = document.querySelector('.hero-section');
    window.addEventListener('scroll', function() {
        const scrollPosition = window.pageYOffset;
        heroSection.style.backgroundPositionY = scrollPosition * 0.5 + 'px';
    });

    // Feature card hover animations
    const featureCards = document.querySelectorAll('.feature-card');
    featureCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-15px)';
            this.style.boxShadow = '0 10px 20px rgba(0, 0, 0, 0.15)';
        });

        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.1)';
        });
    });

    // Dynamic chart placeholder (using Chart.js as an example)
    function createProductionChart() {
        const chartContainer = document.getElementById('productionChart');
        if (!chartContainer) return;

        // This is a placeholder for Chart.js integration
        const ctx = document.createElement('canvas');
        chartContainer.appendChild(ctx);

        // Simulated chart data
        const data = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
            datasets: [{
                label: 'Monthly Production',
                data: [65, 59, 80, 81, 56, 55],
                backgroundColor: 'rgba(0, 123, 255, 0.6)',
                borderColor: 'rgba(0, 123, 255, 1)',
                borderWidth: 1
            }]
        };

        // Note: Actual Chart.js implementation would require the library to be included
        console.log('Chart data prepared', data);
    }

    // Initialize chart
    createProductionChart();

    // Animate stats on scroll
    const statsItems = document.querySelectorAll('.stat-value');
    const observerOptions = {
        threshold: 0.5
    };

    const statsObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateValue(entry.target);
                statsObserver.unobserve(entry.target);
            }
        });
    }, observerOptions);

    statsItems.forEach(item => {
        statsObserver.observe(item);
    });

    // Number animation function
    function animateValue(element) {
        const duration = 2000; // Animation duration in ms
        const startValue = 0;
        const endValue = parseInt(element.textContent);
        const startTime = performance.now();

        function update(currentTime) {
            const elapsedTime = currentTime - startTime;
            const progress = Math.min(elapsedTime / duration, 1);
            const currentValue = Math.floor(progress * (endValue - startValue) + startValue);
            
            element.textContent = currentValue;

            if (progress < 1) {
                requestAnimationFrame(update);
            }
        }

        requestAnimationFrame(update);
    }
});