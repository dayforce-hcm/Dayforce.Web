// TestArea-specific JavaScript

(function() {
    'use strict';
    
    function init() {
        var button = document.querySelector('.demo-button');
        var output = document.querySelector('.demo-output');
        
        if (!button) {
            console.error('TestArea: demo-button element not found');
            return;
        }
        
        if (!output) {
            console.error('TestArea: demo-output element not found');
            return;
        }
        
        button.addEventListener('click', function() {
            var match = button.textContent.match(/Call (.+)/);
            if (!match || !match[1]) {
                output.textContent = 'Error: Could not extract URL from button text';
                output.classList.add('visible');
                console.error('TestArea: Failed to extract URL from button text:', button.textContent);
                return;
            }
            
            var url = match[1];
            
            fetch(url)
                .then(function(response) { return response.json(); })
                .then(function(data) {
                    output.textContent = JSON.stringify(data, null, 2);
                    output.classList.add('visible');
                })
                .catch(function(error) {
                    output.textContent = 'Error: ' + error.message;
                    output.classList.add('visible');
                    console.error('TestArea: Fetch error:', error);
                });
        });
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

