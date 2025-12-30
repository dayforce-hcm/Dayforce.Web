/**
 * Test Dashboard JavaScript
 * Provides client-side functionality for testing ASP.NET MVC framework class endpoints
 */

// Track tested endpoints and their framework classes
const testedEndpoints = new Set();
const allFrameworkClasses = new Set();

/**
 * Initialize collapsible sections
 */
function initCollapsibleSections() {
    var collapsibleSections = document.querySelectorAll('.test-section.collapsible');
    
    collapsibleSections.forEach(function(section) {
        var header = section.querySelector('.section-header');
        var icon = section.querySelector('.toggle-icon');
        
        if (!header) {
            console.error('Collapsible section missing .section-header');
            return;
        }
        
        header.addEventListener('click', function() {
            section.classList.toggle('expanded');
            
            if (section.classList.contains('expanded')) {
                icon.textContent = '-';
            } else {
                icon.textContent = '+';
            }
        });
    });
}

// Initialize on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initCollapsibleSections);
} else {
    initCollapsibleSections();
}

/**
 * Updates the summary stats based on tested endpoints
 */
function updateSummaryStats() {
    const testEndpointsCount = document.getElementById('test-endpoints-count');
    
    if (testEndpointsCount) {
        testEndpointsCount.textContent = testedEndpoints.size;
    }
}

/**
 * Runs all tests on the page sequentially
 */
function runAllTests() {
    const button = document.getElementById('run-all-tests-button');
    const statusElement = document.getElementById('run-all-tests-status');
    
    // Disable the button during test run
    button.disabled = true;
    button.textContent = 'Running Tests...';
    statusElement.textContent = '';
    
    // Find all test buttons on the page (excluding the run-all button)
    const testButtons = Array.from(document.querySelectorAll('.test-button'))
        .filter(btn => btn.id !== 'run-all-tests-button');
    
    if (testButtons.length === 0) {
        statusElement.textContent = 'No tests found';
        button.disabled = false;
        button.textContent = 'Run All Tests';
        return;
    }
    
    statusElement.textContent = `Running 0 of ${testButtons.length} tests...`;
    
    // Run tests sequentially
    let completedCount = 0;
    let successCount = 0;
    let failureCount = 0;
    
    async function runNextTest(index) {
        if (index >= testButtons.length) {
            // All tests complete
            button.disabled = false;
            button.textContent = 'Run All Tests';
            statusElement.textContent = `Complete: ${successCount} passed, ${failureCount} failed`;
            statusElement.style.color = failureCount > 0 ? '#f44336' : '#4caf50';
            return;
        }
        
        const testButton = testButtons[index];
        completedCount++;
        statusElement.textContent = `Running ${completedCount} of ${testButtons.length} tests...`;
        statusElement.style.color = '#666';
        
        // Simulate clicking the button and wait for the test to complete
        try {
            await clickTestButtonAndWait(testButton);
            
            // Check if test was successful by looking at the result element
            const resultElement = testButton.nextElementSibling;
            if (resultElement && resultElement.classList.contains('error')) {
                failureCount++;
            } else {
                successCount++;
            }
        } catch (error) {
            failureCount++;
            console.error('Error running test:', error);
        }
        
        // Small delay between tests to ensure UI updates
        setTimeout(() => runNextTest(index + 1), 100);
    }
    
    runNextTest(0);
}

/**
 * Clicks a test button and waits for the test to complete
 * @param {HTMLElement} button - The test button to click
 * @returns {Promise<void>}
 */
function clickTestButtonAndWait(button) {
    return new Promise((resolve) => {
        // Extract the onclick handler details
        const onclickAttr = button.getAttribute('onclick');
        if (!onclickAttr) {
            resolve();
            return;
        }
        
        // Parse the testEndpoint call from the onclick attribute
        const match = onclickAttr.match(/testEndpoint\((.+)\)/);
        if (!match) {
            resolve();
            return;
        }
        
        // Execute the onclick handler
        button.click();
        
        // Wait a bit for the async fetch to complete
        // We'll use a simple timeout for now
        setTimeout(resolve, 1000);
    });
}

/**
 * Tests an endpoint and displays the result
 * @param {string} path - The endpoint path to test
 * @param {string} method - HTTP method (GET, POST, etc.)
 * @param {object|null} data - Optional data to send with POST requests
 * @param {HTMLElement} button - The button element that triggered the test
 * @param {object|null} headers - Optional custom headers to include in the request
 */
function testEndpoint(path, method, data, button, headers) {
    const resultElement = button.nextElementSibling;
    
    resultElement.style.display = 'block';
    resultElement.textContent = 'Testing...';
    resultElement.classList.remove('error');
    
    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            ...(headers || {})
        }
    };
    
    if (data && method === 'POST') {
        options.body = JSON.stringify(data);
    }
    
    fetch(path, options)
        .then(response => {
            // For file downloads, parse the blob as JSON
            if (path.includes('FileResult')) {
                return response.blob().then(blob => {
                    return blob.text().then(text => {
                        return JSON.parse(text);
                    });
                });
            }
            return response.json();
        })
        .then(data => {
            resultElement.textContent = JSON.stringify(data, null, 2);
            
            // Track this endpoint as tested
            testedEndpoints.add(path);
            
            updateSummaryStats();
        })
        .catch(error => {
            resultElement.classList.add('error');
            resultElement.textContent = 'Error: ' + error.message;
        });
}
