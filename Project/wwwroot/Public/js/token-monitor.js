// Các hằng số
const CONSTANTS = {
    MAX_WARNING_DURATION: 60 * 1000, // 1 phút tối đa để quyết định
    WARNING_THRESHOLD: 60 * 1000, // Cảnh báo trước khi hết hạn 1 phút
    CHECK_INTERVAL: 5 * 1000, // Kiểm tra mỗi 5 giây khi gần hết hạn
    LONG_CHECK_INTERVAL: 30 * 1000, // Kiểm tra mỗi 30 giây khi còn nhiều thời gian
    ONE_MINUTE: 60 * 1000
};

// Trạng thái hệ thống
const state = {
    isWarningDisplayed: false,
    warningStartTime: null,
    warningDialog: null,
    checkInterval: null,
    lastLogTime: null,
    isRenewing: false
};

// Hàm tiện ích
const utils = {
    getTokenExpirationTime() {
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'TokenExpiration') {
                return parseInt(value);
            }
        }
        return null;
    },

    clearInterval() {
        if (state.checkInterval) {
            clearInterval(state.checkInterval);
            state.checkInterval = null;
        }
    },

    setInterval(interval) {
        utils.clearInterval();
        state.checkInterval = setInterval(startTokenMonitoring, interval);
    },

    logTimeRemaining(timeUntilExpiration) {
        const now = Date.now();
        if (!state.lastLogTime || (now - state.lastLogTime) >= 1000) {
            const seconds = Math.floor(timeUntilExpiration / 1000);
            console.log(`Thời gian còn lại: ${seconds} giây`);
            state.lastLogTime = now;
        }
    }
};

// Hàm xử lý token
const tokenHandler = {
    async renew() {
        if (state.isRenewing) return false;
        state.isRenewing = true;
        
        try {
            const response = await fetch('/api/token/renew', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            
            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    // Reset all states
                    if (state.warningDialog) {
                        state.warningDialog.close();
                        state.warningDialog = null;
                    }
                    state.isWarningDisplayed = false;
                    state.warningStartTime = null;
                    state.lastLogTime = null;
                    state.isRenewing = false;
                    
                    // Clear existing interval
                    utils.clearInterval();
                    
                    // Wait for the new cookie to be set
                    await new Promise(resolve => setTimeout(resolve, 500));
                    
                    // Start monitoring with new token
                    startTokenMonitoring();
                    return true;
                }
            }
            state.isRenewing = false;
            return false;
        } catch (error) {
            state.isRenewing = false;
            return false;
        }
    },

    showWarningDialog() {
        if (state.isRenewing) return;
        
        if (typeof $.confirm === 'undefined') {
            if (confirm('Thời gian làm việc trên web sắp hết, bạn có muốn tiếp tục không?')) {
                tokenHandler.renew();
            } else {
                tokenHandler.logout();
            }
            return;
        }

        state.warningDialog = $.confirm({
            title: 'Cảnh báo hết phiên làm việc',
            content: '<div style="font-size: 16px; line-height: 1.5;">' +
                    'Thời gian làm việc trên web sắp hết, bạn có muốn tiếp tục không?' +
                    '<br/><br/>' +
                    '<div style="font-size: 14px; color: #666;">' +
                    'Bạn có 1 phút để quyết định, sau đó hệ thống sẽ tự động đăng xuất.' +
                    '</div></div>',
            type: 'orange',
            icon: 'fa fa-exclamation-triangle',
            theme: 'modern',
            boxWidth: '400px',
            useBootstrap: false,
            typeAnimated: true,
            buttons: {
                yes: {
                    text: 'Có',
                    btnClass: 'btn-blue',
                    action: async function() {
                        const renewed = await tokenHandler.renew();
                        if (!renewed) {
                            tokenHandler.logout();
                        }
                    }
                },
                no: {
                    text: 'Không',
                    btnClass: 'btn-red',
                    action: function() {
                        tokenHandler.logout();
                    }
                }
            },
            closeIcon: false,
            closeOnEscape: false,
            closeOnClick: false
        });

        // Set timeout to automatically logout after 1 minute
        setTimeout(() => {
            if (state.isWarningDisplayed && state.warningDialog) {
                state.warningDialog.close();
                tokenHandler.logout();
            }
        }, CONSTANTS.MAX_WARNING_DURATION);
    },

    logout() {
        // Xóa tất cả cookie liên quan
        document.cookie = 'AuthToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
        document.cookie = 'TokenExpiration=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
        
        // Gọi API logout và chuyển hướng
        fetch('/dang-xuat', {
            method: 'GET',
            redirect: 'follow'
        }).then(response => {
            if (response.redirected) {
                window.location.replace(response.url);
            }
        });
    }
};

// Hàm chính để theo dõi token
function startTokenMonitoring() {
    const expirationTime = utils.getTokenExpirationTime();
    if (!expirationTime) return;

    const now = Date.now();
    const timeUntilExpiration = expirationTime - now;

    if (state.isWarningDisplayed || state.isRenewing) return;

    // Điều chỉnh tần suất kiểm tra dựa trên thời gian còn lại
    if (timeUntilExpiration <= CONSTANTS.ONE_MINUTE) {
        utils.setInterval(CONSTANTS.CHECK_INTERVAL);
    } else {
        utils.setInterval(CONSTANTS.LONG_CHECK_INTERVAL);
    }

    // Hiển thị cảnh báo khi cần
    if (timeUntilExpiration <= CONSTANTS.WARNING_THRESHOLD && !state.isWarningDisplayed && !state.isRenewing) {
        state.isWarningDisplayed = true;
        state.warningStartTime = now;
        tokenHandler.showWarningDialog();
    }
}

// Khởi động hệ thống
document.addEventListener('DOMContentLoaded', startTokenMonitoring);
window.addEventListener('beforeunload', utils.clearInterval); 