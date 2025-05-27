// Function to create pie chart configuration
function createChartOptions(data) {
    if (!data || data.length === 0) {
        return {
            series: [1],
            chart: {
                type: 'pie',
                height: 250
            },
            labels: ['Chưa có dữ liệu'],
            colors: ['#E0E0E0'],
            legend: {
                show: false
            },
            dataLabels: {
                enabled: true,
                formatter: function () {
                    return 'Chưa có dữ liệu'
                },
                style: {
                    fontSize: '16px',
                    fontFamily: 'inherit'
                }
            },
            tooltip: {
                enabled: false
            },
            states: {
                hover: {
                    filter: {
                        type: 'none'
                    }
                }
            }
        };
    }

    return {
        series: data.map(item => item.totalTreatments),
        chart: {
            type: 'pie',
            height: 300
        },
        labels: data.map(item => item.methodName),
        colors: [
            '#4CAF50', // Xanh lá
            '#2196F3', // Xanh dương
            '#FFC107', // Vàng
            '#E91E63', // Hồng
            '#9C27B0', // Tím
            '#FF9800', // Cam
            '#795548', // Nâu
            '#607D8B', // Xám xanh
            '#00BCD4', // Xanh ngọc
            '#8BC34A'  // Xanh lá nhạt
        ],
        legend: {
            position: 'bottom',
            labels: {
                colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
            }
        },
        dataLabels: {
            enabled: true,
            formatter: function (val, opts) {
                return opts.w.config.series[opts.seriesIndex] + ' lượt'
            }
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val + ' lượt điều trị'
                }
            }
        }
    };
}

// Function to create donut chart configuration
function createDonutChartOptions(data) {
    if (!data || data.length === 0) {
        return {
            series: [1],
            chart: {
                type: 'donut',
                height: 380
            },
            labels: ['Chưa có dữ liệu'],
            colors: ['#E0E0E0'],
            legend: {
                show: false
            },
            dataLabels: {
                enabled: true,
                formatter: function () {
                    return 'Chưa có dữ liệu'
                },
                style: {
                    fontSize: '16px',
                    fontFamily: 'inherit'
                }
            },
            tooltip: {
                enabled: false
            },
            states: {
                hover: {
                    filter: {
                        type: 'none'
                    }
                }
            }
        };
    }

    return {
        series: data.map(item => item.patientCount),
        chart: {
            type: 'donut',
            height: 380
        },
        labels: data.map(item => item.roomName),
        colors: ['#4CAF50', '#2196F3', '#FFC107', '#E91E63', '#9C27B0', '#FF9800', '#795548', '#607D8B'],
        legend: {
            position: 'bottom',
            labels: {
                colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
            }
        },
        dataLabels: {
            enabled: true,
            formatter: function (val, opts) {
                return opts.w.config.series[opts.seriesIndex] + ' bệnh nhân'
            }
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val + ' bệnh nhân'
                }
            }
        },
        plotOptions: {
            pie: {
                donut: {
                    size: '65%',
                    labels: {
                        show: true,
                        total: {
                            show: true,
                            label: 'Tổng',
                            formatter: function (w) {
                                return w.globals.seriesTotals.reduce((a, b) => a + b, 0) + ' bệnh nhân'
                            }
                        }
                    }
                }
            }
        }
    };
}

// Function to create and render a chart for a specific department
async function createDepartmentChart(elementId, departmentCode, startDate, endDate) {
    let url = `/Staff/Statistics/GetTreatmentStatsByDepartment?departmentCode=${departmentCode}`;
    if (startDate) url += `&startDate=${startDate}`;
    if (endDate) url += `&endDate=${endDate}`;
    const response = await fetch(url);
    const data = await response.json();

    const chartContainer = document.querySelector(`#${elementId}`);
    // Destroy old chart if exists
    if (chartContainer.__chartInstance) {
        chartContainer.__chartInstance.destroy();
    }

    const chart = new ApexCharts(
        chartContainer,
        createChartOptions(data)
    );
    chartContainer.__chartInstance = chart; // Save instance for later destroy
    chart.render();
}

// Function to load and render patient admission chart
async function loadPatientAdmissionStats(startDate, endDate, groupBy) {
    try {
        if (!startDate || !endDate) {
            ({ start: startDate, end: endDate } = getDateRange());
        }
        const response = await fetch(`/Staff/Statistics/GetPatientAdmissionStats?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&groupBy=${groupBy}`);
        const data = await response.json();

        // Tạo mảng các mốc thời gian đầy đủ
        let categories = [];
        let datePointer = new Date(startDate);
        let end = new Date(endDate);

        if (groupBy === 'day') {
            while (datePointer <= end) {
                categories.push(datePointer.toISOString().slice(0, 10)); // yyyy-mm-dd
                datePointer.setDate(datePointer.getDate() + 1);
            }
        } else if (groupBy === 'month') {
            while (datePointer <= end) {
                categories.push(datePointer.toISOString().slice(0, 7)); // yyyy-mm
                datePointer.setMonth(datePointer.getMonth() + 1);
            }
        } else if (groupBy === 'year') {
            while (datePointer <= end) {
                categories.push(datePointer.getFullYear().toString());
                datePointer.setFullYear(datePointer.getFullYear() + 1);
            }
        }

        // Map dữ liệu từ API vào mảng này, nếu không có thì gán 0
        const dataMap = {};
        data.forEach(item => {
            let key;
            if (groupBy === 'day') key = item.date.slice(0, 10);
            else if (groupBy === 'month') key = item.date.slice(0, 7);
            else if (groupBy === 'year') key = item.date.slice(0, 4);
            dataMap[key] = Number(item.patientCount) || 0;
        });

        const seriesData = categories.map(key => dataMap[key] || 0);
        let xLabels = categories.map(key => {
            if (groupBy === 'day') {
                const [y, m, d] = key.split('-');
                return `${d}/${m}/${y}`;
            } else if (groupBy === 'month') {
                const [y, m] = key.split('-');
                return `${m}/${y}`;
            } else {
                return key;
            }
        });

        // Destroy existing chart if any
        const existingChart = document.querySelector('#patientAdmissionChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }

        const options = {
            series: [{
                name: 'Số lượng bệnh nhân',
                data: seriesData
            }],
            chart: {
                type: 'bar',
                height: 350,
                zoom: { enabled: true }
            },
            dataLabels: { enabled: false },
            stroke: { width: 3 },
            colors: ['#4361ee'],
            xaxis: {
                categories: xLabels,
                labels: {
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    },
                    rotate: -45,
                    rotateAlways: false
                }
            },
            yaxis: {
                title: {
                    text: 'Số lượng bệnh nhân',
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    }
                },
                labels: {
                    formatter: function (val) {
                        return parseInt(val);
                    },
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    }
                }
            },
            tooltip: {
                y: {
                    formatter: function (val) {
                        return val + ' bệnh nhân'
                    }
                }
            }
        };

        const chart = new ApexCharts(existingChart, options);
        existingChart.__chartInstance = chart;
        chart.render();
    } catch (error) {
        console.error('Lỗi khi tải thống kê nhập viện: ', error);
    }
}

// Function to load and render treatment completion chart
async function loadTreatmentCompletionStats(startDate, endDate, groupBy) {
    try {
        if (!startDate || !endDate) {
            ({ start: startDate, end: endDate } = getDateRange());
        }
        const response = await fetch(`/Staff/Statistics/GetTreatmentCompletionStats?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&groupBy=${groupBy}`);
        const data = await response.json();

        // Tạo mảng các mốc thời gian đầy đủ
        let categories = [];
        let datePointer = new Date(startDate);
        let end = new Date(endDate);

        if (groupBy === 'day') {
            while (datePointer <= end) {
                categories.push(datePointer.toISOString().slice(0, 10)); // yyyy-mm-dd
                datePointer.setDate(datePointer.getDate() + 1);
            }
        } else if (groupBy === 'month') {
            while (datePointer <= end) {
                categories.push(datePointer.toISOString().slice(0, 7)); // yyyy-mm
                datePointer.setMonth(datePointer.getMonth() + 1);
            }
        } else if (groupBy === 'year') {
            while (datePointer <= end) {
                categories.push(datePointer.getFullYear().toString());
                datePointer.setFullYear(datePointer.getFullYear() + 1);
            }
        }

        // Map dữ liệu từ API vào mảng này, nếu không có thì gán 0
        const dataMap = {};
        data.forEach(item => {
            let key;
            if (groupBy === 'day') key = item.date.slice(0, 10);
            else if (groupBy === 'month') key = item.date.slice(0, 7);
            else if (groupBy === 'year') key = item.date.slice(0, 4);
            dataMap[key] = item;
        });

        const completedSeries = categories.map(key => dataMap[key]?.completedCount || 0);
        const cancelledSeries = categories.map(key => dataMap[key]?.cancelledCount || 0);
        const xLabels = categories.map(key => {
            if (groupBy === 'day') {
                const [y, m, d] = key.split('-');
                return `${d}/${m}/${y}`;
            } else if (groupBy === 'month') {
                const [y, m] = key.split('-');
                return `${m}/${y}`;
            } else {
                return key;
            }
        });

        // Destroy existing chart if any
        const existingChart = document.querySelector('#treatmentCompletionChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }

        const options = {
            series: [
                { name: 'Đã hoàn thành', data: completedSeries },
                { name: 'Đã hủy bỏ', data: cancelledSeries }
            ],
            chart: {
                type: 'bar',
                height: 250,
                stacked: false,
                toolbar: { show: true }
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    columnWidth: '50%',
                    borderRadius: 4,
                    dataLabels: { position: 'top' }
                }
            },
            dataLabels: { enabled: false },
            xaxis: {
                categories: xLabels,
                labels: {
                    style: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' },
                    rotate: -45,
                    rotateAlways: false
                }
            },
            yaxis: {
                title: {
                    text: 'Số lượng đợt điều trị',
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    }
                },
                labels: {
                    formatter: function (val) {
                        return parseInt(val);
                    },
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    }
                }
            },
            colors: ['#4CAF50', '#FF5252'],
            tooltip: { y: { formatter: val => val + ' đợt điều trị' } }
        };

        const chart = new ApexCharts(existingChart, options);
        existingChart.__chartInstance = chart;
        chart.render();
    } catch (error) {
        console.error('Lỗi khi tải thống kê điều trị: ', error);
    }
}

// Function to render donut chart for suspended reasons
async function loadSuspendedReasonStats(startDate, endDate, groupBy) {
    let url = '/Staff/Statistics/GetSuspendedReasonStats';
    if (startDate && endDate && groupBy) {
        url += `?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&groupBy=${groupBy}`;
    }
    const response = await fetch(url);
    const data = await response.json();
    const chartContainer = document.querySelector('#suspendedReasonChart');
    if (chartContainer.__chartInstance) {
        chartContainer.__chartInstance.destroy();
    }
    const options = {
        series: data.map(x => x.count),
        chart: { type: 'donut', height: 320 },
        labels: data.map(x => x.reason),
        colors: ['#FF9800', '#4CAF50', '#9C27B0'],
        legend: { position: 'bottom', labels: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' } },
        dataLabels: { enabled: true, formatter: (val, opts) => opts.w.config.series[opts.seriesIndex] + ' lượt' },
        tooltip: { y: { formatter: val => val + ' lượt' } },
        plotOptions: { pie: { donut: { size: '65%', labels: { show: true, total: { show: true, label: 'Tổng', formatter: w => w.globals.seriesTotals.reduce((a, b) => a + b, 0) + ' lượt' } } } } }
    };
    // Nếu không có dữ liệu
    if (!data || data.length === 0) {
        options.series = [1];
        options.labels = ['Chưa có dữ liệu'];
        options.colors = ['#E0E0E0'];
        options.legend.show = false;
        options.dataLabels.formatter = () => 'Chưa có dữ liệu';
        options.tooltip.enabled = false;
    }
    const chart = new ApexCharts(chartContainer, options);
    chartContainer.__chartInstance = chart;
    chart.render();
}

// Function to load and render patient type chart (new/old)
async function loadPatientTypeStats(startDate, endDate, groupBy) {
    let url = '/Staff/Statistics/GetPatientTypeStats';
    if (startDate && endDate && groupBy) {
        url += `?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&groupBy=${groupBy}`;
    }
    const response = await fetch(url);
    const data = await response.json();
    // Tổng hợp tổng số bệnh nhân mới/cũ trong toàn bộ khoảng lọc
    let totalNew = 0;
    let totalOld = 0;
    data.forEach(item => {
        totalNew += item.newCount || 0;
        totalOld += item.oldCount || 0;
    });
    const chartContainer = document.querySelector('#patientTypeChart');
    if (chartContainer.__chartInstance) {
        chartContainer.__chartInstance.destroy();
    }
    const options = {
        series: [totalNew, totalOld],
        chart: { type: 'donut', height: 350 },
        labels: ['Bệnh nhân mới', 'Bệnh nhân cũ'],
        colors: ['#ff99cc', '#cc99ff'],
        legend: { position: 'bottom', labels: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' } },
        dataLabels: { enabled: true, formatter: (val, opts) => opts.w.config.series[opts.seriesIndex] + ' bệnh nhân' },
        tooltip: { y: { formatter: val => val + ' bệnh nhân' } },
        plotOptions: { pie: { donut: { size: '70%', labels: { show: true, total: { show: true, label: 'Tổng', formatter: w => w.globals.seriesTotals.reduce((a, b) => a + b, 0) + ' bệnh nhân' } } } } }
    };
    // Nếu không có dữ liệu
    if (totalNew + totalOld === 0) {
        options.series = [1];
        options.labels = ['Chưa có dữ liệu'];
        options.colors = ['#E0E0E0'];
        options.legend.show = false;
        options.dataLabels.formatter = () => 'Chưa có dữ liệu';
        options.tooltip.enabled = false;
        options.plotOptions.pie.donut.labels.total.formatter = () => 'Chưa có dữ liệu';
    }
    const chart = new ApexCharts(chartContainer, options);
    chartContainer.__chartInstance = chart;
    chart.render();
}

// Function to load and render revenue chart
async function loadRevenueStats(startDate, endDate, groupBy) {
    try {
        // Nếu không truyền startDate/endDate thì lấy 7 ngày: hôm nay ở giữa
        if (!startDate || !endDate) {
            ({ start: startDate, end: endDate } = getDateRange());
        }
        const response = await fetch(`/Staff/Statistics/GetRevenueStats?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&groupBy=${groupBy}`);
        const data = await response.json();

        // Tạo mảng các mốc thời gian đầy đủ
        let categories = [];
        let datePointer = new Date(startDate);
        let end = new Date(endDate);

        if (groupBy === 'day') {
            while (datePointer <= end) {
                categories.push(datePointer.toISOString().slice(0, 10)); // yyyy-mm-dd
                datePointer.setDate(datePointer.getDate() + 1);
            }
        } else if (groupBy === 'month') {
            while (datePointer <= end) {
                categories.push(datePointer.toISOString().slice(0, 7)); // yyyy-mm
                datePointer.setMonth(datePointer.getMonth() + 1);
            }
        } else if (groupBy === 'year') {
            while (datePointer <= end) {
                categories.push(datePointer.getFullYear().toString());
                datePointer.setFullYear(datePointer.getFullYear() + 1);
            }
        }

        // Map dữ liệu từ API vào mảng này, nếu không có thì gán 0
        const dataMap = {};
        data.forEach(item => {
            let key;
            if (groupBy === 'day') key = item.date.slice(0, 10);
            else if (groupBy === 'month') key = item.date.slice(0, 7);
            else if (groupBy === 'year') key = item.date.slice(0, 4);
            dataMap[key] = item.revenue;
        });

        const seriesData = categories.map(key => dataMap[key] || 0);
        let xLabels = categories.map(key => {
            if (groupBy === 'day') {
                const [y, m, d] = key.split('-');
                return `${d}/${m}/${y}`;
            } else if (groupBy === 'month') {
                const [y, m] = key.split('-');
                return `${m}/${y}`;
            } else {
                return key;
            }
        });

        // Destroy existing chart if any
        const existingChart = document.querySelector('#revenueChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }

        const options = {
            series: [{
                name: 'Doanh thu (VNĐ)',
                data: seriesData
            }],
            chart: {
                type: 'bar',
                height: 350,
                zoom: { enabled: true }
            },
            dataLabels: { enabled: false },
            stroke: { width: 3 },
            colors: ['#00b894'],
            xaxis: {
                categories: xLabels,
                labels: {
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    },
                    rotate: -45,
                    rotateAlways: false
                }
            },
            yaxis: {
                title: {
                    text: 'Doanh thu (VNĐ)',
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    }
                },
                labels: {
                    formatter: function (val) {
                        return val.toLocaleString();
                    },
                    style: {
                        colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000'
                    }
                }
            },
            tooltip: {
                y: {
                    formatter: function (val) {
                        return val.toLocaleString() + ' VNĐ'
                    }
                }
            }
        };

        const chart = new ApexCharts(existingChart, options);
        existingChart.__chartInstance = chart;
        chart.render();
    } catch (error) {
        console.error('Lỗi khi tải thống kê doanh thu: ', error);
    }
}

// Biểu đồ số lượng phiếu chưa thanh toán
async function loadUnpaidPaymentCountStats(startDate, endDate, groupBy) {
    if (!startDate || !endDate) {
        const range = getDateRange();
        startDate = range.start;
        endDate = range.end;
    }
    const response = await fetch(`/Staff/Statistics/GetUnpaidPaymentCountStats?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&groupBy=${groupBy}`);
    const data = await response.json();

    // Tạo mảng các mốc thời gian đầy đủ
    let categories = [];
    let datePointer = new Date(startDate);
    let end = new Date(endDate);

    if (groupBy === 'day') {
        while (datePointer <= end) {
            categories.push(datePointer.toISOString().slice(0, 10));
            datePointer.setDate(datePointer.getDate() + 1);
        }
    } else if (groupBy === 'month') {
        while (datePointer <= end) {
            categories.push(datePointer.toISOString().slice(0, 7));
            datePointer.setMonth(datePointer.getMonth() + 1);
        }
    } else if (groupBy === 'year') {
        while (datePointer <= end) {
            categories.push(datePointer.getFullYear().toString());
            datePointer.setFullYear(datePointer.getFullYear() + 1);
        }
    }

    const dataMap = {};
    data.forEach(item => {
        let key;
        if (groupBy === 'day') key = item.date.slice(0, 10);
        else if (groupBy === 'month') key = item.date.slice(0, 7);
        else if (groupBy === 'year') key = item.date.slice(0, 4);
        dataMap[key] = item.count;
    });

    const seriesData = categories.map(key => dataMap[key] || 0);
    let xLabels = categories.map(key => {
        if (groupBy === 'day') {
            const [y, m, d] = key.split('-');
            return `${d}/${m}/${y}`;
        } else if (groupBy === 'month') {
            const [y, m] = key.split('-');
            return `${m}/${y}`;
        } else {
            return key;
        }
    });

    const chartContainer = document.querySelector('#unpaidPaymentChart');
    if (chartContainer.__chartInstance) chartContainer.__chartInstance.destroy();

    const options = {
        series: [{ name: 'Số phiếu chưa thanh toán', data: seriesData }],
        chart: { type: 'bar', height: 350 },
        dataLabels: { enabled: false },
        colors: ['#fd7e14'],
        xaxis: {
            categories: xLabels,
            labels: { style: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' }, rotate: -45 }
        },
        yaxis: {
            title: { text: 'Số phiếu', style: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' } },
            labels: { formatter: val => parseInt(val), style: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' } }
        },
        tooltip: { y: { formatter: val => val + ' phiếu' } }
    };

    const chart = new ApexCharts(chartContainer, options);
    chartContainer.__chartInstance = chart;
    chart.render();
}

// Biểu đồ tổng số tiền phiếu chưa thanh toán
async function loadUnpaidPaymentAmountStats(startDate, endDate, groupBy) {
    if (!startDate || !endDate) {
        ({ start: startDate, end: endDate } = getDateRange());
    }
    const response = await fetch(`/Staff/Statistics/GetUnpaidPaymentAmountStats?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}&groupBy=${groupBy}`);
    const data = await response.json();

    // Tạo mảng các mốc thời gian đầy đủ
    let categories = [];
    let datePointer = new Date(startDate);
    let end = new Date(endDate);

    if (groupBy === 'day') {
        while (datePointer <= end) {
            categories.push(datePointer.toISOString().slice(0, 10));
            datePointer.setDate(datePointer.getDate() + 1);
        }
    } else if (groupBy === 'month') {
        while (datePointer <= end) {
            categories.push(datePointer.toISOString().slice(0, 7));
            datePointer.setMonth(datePointer.getMonth() + 1);
        }
    } else if (groupBy === 'year') {
        while (datePointer <= end) {
            categories.push(datePointer.getFullYear().toString());
            datePointer.setFullYear(datePointer.getFullYear() + 1);
        }
    }

    const dataMap = {};
    data.forEach(item => {
        let key;
        if (groupBy === 'day') key = item.date.slice(0, 10);
        else if (groupBy === 'month') key = item.date.slice(0, 7);
        else if (groupBy === 'year') key = item.date.slice(0, 4);
        dataMap[key] = item.amount;
    });

    const seriesData = categories.map(key => dataMap[key] || 0);
    let xLabels = categories.map(key => {
        if (groupBy === 'day') {
            const [y, m, d] = key.split('-');
            return `${d}/${m}/${y}`;
        } else if (groupBy === 'month') {
            const [y, m] = key.split('-');
            return `${m}/${y}`;
        } else {
            return key;
        }
    });

    const chartContainer = document.querySelector('#unpaidPaymentAmountChart');
    if (chartContainer.__chartInstance) chartContainer.__chartInstance.destroy();

    const options = {
        series: [{ name: 'Tổng số tiền chưa thanh toán (VNĐ)', data: seriesData }],
        chart: { type: 'line', height: 350, zoom: { enabled: true } },
        dataLabels: { enabled: false },
        stroke: { width: 3, curve: 'smooth' },
        colors: ['#e74c3c'],
        xaxis: {
            categories: xLabels,
            labels: { style: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' }, rotate: -45 }
        },
        yaxis: {
            title: { text: 'Số tiền (VNĐ)', style: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' } },
            labels: { formatter: val => val.toLocaleString(), style: { colors: document.documentElement.classList.contains('dark') ? '#fff' : '#000' } }
        },
        tooltip: { y: { formatter: val => val.toLocaleString() + ' VNĐ' } }
    };

    const chart = new ApexCharts(chartContainer, options);
    chartContainer.__chartInstance = chart;
    chart.render();
}

document.addEventListener('DOMContentLoaded', function () {
    // Load all charts when the page loads
    loadAllStats();

    // Department filter logic
    const timeTypeSelect = document.getElementById('departmentTimeType');
    const dateRangeDiv = document.getElementById('departmentDateRange');
    let startInput = document.getElementById('departmentStartDate');
    let endInput = document.getElementById('departmentEndDate');
    const resetBtn = document.getElementById('departmentResetBtn');

    // Admission filter logic
    const admissionTimeType = document.getElementById('admissionTimeType');
    const admissionDateRange = document.getElementById('admissionDateRange');
    let admissionStartInput = document.getElementById('admissionStartDate');
    let admissionEndInput = document.getElementById('admissionEndDate');
    const admissionResetBtn = document.getElementById('admissionResetBtn');

    // Treatment filter logic
    const treatmentTimeType = document.getElementById('treatmentTimeType');
    const treatmentDateRange = document.getElementById('treatmentDateRange');
    let treatmentStartInput = document.getElementById('treatmentStartDate');
    let treatmentEndInput = document.getElementById('treatmentEndDate');
    const treatmentResetBtn = document.getElementById('treatmentResetBtn');

    // Patient type filter logic
    const patientTypeTimeType = document.getElementById('patientTypeTimeType');
    const patientTypeDateRange = document.getElementById('patientTypeDateRange');
    let patientTypeStartInput = document.getElementById('patientTypeStartDate');
    let patientTypeEndInput = document.getElementById('patientTypeEndDate');
    const patientTypeResetBtn = document.getElementById('patientTypeResetBtn');

    // Revenue filter logic
    const revenueTimeType = document.getElementById('revenueTimeType');
    const revenueDateRange = document.getElementById('revenueDateRange');
    let revenueStartInput = document.getElementById('revenueStartDate');
    let revenueEndInput = document.getElementById('revenueEndDate');
    const revenueResetBtn = document.getElementById('revenueResetBtn');

    // Unpaid payment filter logic
    const unpaidPaymentTimeType = document.getElementById('unpaidPaymentTimeType');
    const unpaidPaymentDateRange = document.getElementById('unpaidPaymentDateRange');
    let unpaidPaymentStartInput = document.getElementById('unpaidPaymentStartDate');
    let unpaidPaymentEndInput = document.getElementById('unpaidPaymentEndDate');
    const unpaidPaymentResetBtn = document.getElementById('unpaidPaymentResetBtn');

    // Unpaid payment amount filter logic
    const unpaidPaymentAmountTimeType = document.getElementById('unpaidPaymentAmountTimeType');
    const unpaidPaymentAmountDateRange = document.getElementById('unpaidPaymentAmountDateRange');
    let unpaidPaymentAmountStartInput = document.getElementById('unpaidPaymentAmountStartDate');
    let unpaidPaymentAmountEndInput = document.getElementById('unpaidPaymentAmountEndDate');
    const unpaidPaymentAmountResetBtn = document.getElementById('unpaidPaymentAmountResetBtn');

    // Update reset button state
    function updateResetBtnState() {
        if (startInput.value && endInput.value) {
            resetBtn.disabled = false;
        } else {
            resetBtn.disabled = true;
        }
    }
    function updateAdmissionResetBtnState() {
        if (admissionStartInput.value && admissionEndInput.value) {
            admissionResetBtn.disabled = false;
        } else {
            admissionResetBtn.disabled = true;
        }
    }
    function updateTreatmentResetBtnState() {
        if (treatmentStartInput.value && treatmentEndInput.value) {
            treatmentResetBtn.disabled = false;
        } else {
            treatmentResetBtn.disabled = true;
        }
    }
    function updatePatientTypeResetBtnState() {
        if (patientTypeStartInput.value && patientTypeEndInput.value) {
            patientTypeResetBtn.disabled = false;
        } else {
            patientTypeResetBtn.disabled = true;
        }
    }

    function updateRevenueResetBtnState() {
        if (revenueStartInput.value && revenueEndInput.value) {
            revenueResetBtn.disabled = false;
        } else {
            revenueResetBtn.disabled = true;
        }
    }

    function updateUnpaidPaymentResetBtnState() {
        if (unpaidPaymentStartInput.value && unpaidPaymentEndInput.value) {
            unpaidPaymentResetBtn.disabled = false;
        } else {
            unpaidPaymentResetBtn.disabled = true;
        }
    }

    function updateUnpaidPaymentAmountResetBtnState() {
        if (unpaidPaymentAmountStartInput.value && unpaidPaymentAmountEndInput.value) {
            unpaidPaymentAmountResetBtn.disabled = false;
        } else {
            unpaidPaymentAmountResetBtn.disabled = true;
        }
    }

    // Populate year select
    function populateYearSelect(select) {
        const currentYear = new Date().getFullYear();
        select.innerHTML = '<option value=""></option>';
        for (let y = currentYear; y >= 2000; y--) {
            select.innerHTML += `<option value="${y}">${y}</option>`;
        }
    }

    // Format date for display
    function formatDate(date, groupBy) {
        const options = {
            day: { day: '2-digit', month: '2-digit', year: 'numeric' },
            week: { day: '2-digit', month: '2-digit', year: 'numeric' },
            month: { month: 'long', year: 'numeric' },
            year: { year: 'numeric' }
        };
        return new Date(date).toLocaleDateString('vi-VN', options[groupBy]);
    }

    // Event listeners
    startInput.addEventListener('input', updateResetBtnState);
    endInput.addEventListener('input', updateResetBtnState);
    admissionStartInput.addEventListener('input', updateAdmissionResetBtnState);
    admissionEndInput.addEventListener('input', updateAdmissionResetBtnState);
    treatmentStartInput.addEventListener('input', updateTreatmentResetBtnState);
    treatmentEndInput.addEventListener('input', updateTreatmentResetBtnState);
    patientTypeStartInput.addEventListener('input', updatePatientTypeResetBtnState);
    patientTypeEndInput.addEventListener('input', updatePatientTypeResetBtnState);
    revenueStartInput.addEventListener('input', updateRevenueResetBtnState);
    revenueEndInput.addEventListener('input', updateRevenueResetBtnState);
    unpaidPaymentStartInput.addEventListener('input', updateUnpaidPaymentResetBtnState);
    unpaidPaymentEndInput.addEventListener('input', updateUnpaidPaymentResetBtnState);
    unpaidPaymentAmountStartInput.addEventListener('input', updateUnpaidPaymentAmountResetBtnState);
    unpaidPaymentAmountEndInput.addEventListener('input', updateUnpaidPaymentAmountResetBtnState);

    timeTypeSelect.addEventListener('change', function () {
        const type = this.value;
        if (!type) {
            dateRangeDiv.style.display = 'none';
            startInput.value = endInput.value = '';
            updateResetBtnState();
            // Reset chart về mặc định
            createDepartmentChart('ccdsChart', 'F75HS667');
            createDepartmentChart('vltlChart', '42H35AXU');
            createDepartmentChart('bnctChart', 'HZWIPN7U');
        } else {
            dateRangeDiv.style.display = '';
            // Xóa event cũ
            startInput.removeEventListener('input', updateResetBtnState);
            endInput.removeEventListener('input', updateResetBtnState);
            // Thay input phù hợp
            if (type === 'day') {
                if (startInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'date';
                    newStart.id = 'departmentStartDate';
                    newStart.className = 'form-input text-sm';
                    startInput.replaceWith(newStart);
                    startInput = newStart;
                } else {
                    startInput.type = 'date';
                }
                if (endInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'date';
                    newEnd.id = 'departmentEndDate';
                    newEnd.className = 'form-input text-sm';
                    endInput.replaceWith(newEnd);
                    endInput = newEnd;
                } else {
                    endInput.type = 'date';
                }
                startInput.pattern = endInput.pattern = '';
            } else if (type === 'month') {
                if (startInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'month';
                    newStart.id = 'departmentStartDate';
                    newStart.className = 'form-input text-sm';
                    startInput.replaceWith(newStart);
                    startInput = newStart;
                } else {
                    startInput.type = 'month';
                }
                if (endInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'month';
                    newEnd.id = 'departmentEndDate';
                    newEnd.className = 'form-input text-sm';
                    endInput.replaceWith(newEnd);
                    endInput = newEnd;
                } else {
                    endInput.type = 'month';
                }
                startInput.pattern = endInput.pattern = '';
            } else if (type === 'year') {
                if (startInput.tagName.toLowerCase() !== 'select') {
                    const newStart = document.createElement('select');
                    newStart.id = 'departmentStartDate';
                    newStart.className = 'form-input text-sm';
                    startInput.replaceWith(newStart);
                    startInput = newStart;
                }
                if (endInput.tagName.toLowerCase() !== 'select') {
                    const newEnd = document.createElement('select');
                    newEnd.id = 'departmentEndDate';
                    newEnd.className = 'form-input text-sm';
                    endInput.replaceWith(newEnd);
                    endInput = newEnd;
                }
                populateYearSelect(startInput);
                populateYearSelect(endInput);
                startInput.firstElementChild.text = 'Chọn năm bắt đầu';
                endInput.firstElementChild.text = 'Chọn năm kết thúc';
            }
            startInput.value = endInput.value = '';
            startInput.addEventListener('input', updateResetBtnState);
            endInput.addEventListener('input', updateResetBtnState);
            updateResetBtnState();
            // Reset chart về mặc định khi đổi loại thời gian
            createDepartmentChart('ccdsChart', 'F75HS667');
            createDepartmentChart('vltlChart', '42H35AXU');
            createDepartmentChart('bnctChart', 'HZWIPN7U');
        }
    });

    admissionTimeType.addEventListener('change', function () {
        const type = this.value;
        if (!type) {
            admissionDateRange.style.display = 'none';
            admissionStartInput.value = admissionEndInput.value = '';
            updateAdmissionResetBtnState();
            ({ start: admissionStartDate, end: admissionEndDate } = getDateRange());
            loadPatientAdmissionStats(admissionStartDate, admissionEndDate, 'day');
        } else {
            admissionDateRange.style.display = '';
            // Xóa event cũ
            admissionStartInput.removeEventListener('input', updateAdmissionResetBtnState);
            admissionEndInput.removeEventListener('input', updateAdmissionResetBtnState);
            // Thay input phù hợp
            if (type === 'day') {
                if (admissionStartInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'date';
                    newStart.id = 'admissionStartDate';
                    newStart.className = 'form-input text-sm';
                    admissionStartInput.replaceWith(newStart);
                    admissionStartInput = newStart;
                } else {
                    admissionStartInput.type = 'date';
                }
                if (admissionEndInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'date';
                    newEnd.id = 'admissionEndDate';
                    newEnd.className = 'form-input text-sm';
                    admissionEndInput.replaceWith(newEnd);
                    admissionEndInput = newEnd;
                } else {
                    admissionEndInput.type = 'date';
                }
                admissionStartInput.placeholder = admissionEndInput.placeholder = 'dd/mm/yyyy';
                admissionStartInput.pattern = admissionEndInput.pattern = '';
            } else if (type === 'month') {
                if (admissionStartInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'month';
                    newStart.id = 'admissionStartDate';
                    newStart.className = 'form-input text-sm';
                    admissionStartInput.replaceWith(newStart);
                    admissionStartInput = newStart;
                } else {
                    admissionStartInput.type = 'month';
                }
                if (admissionEndInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'month';
                    newEnd.id = 'admissionEndDate';
                    newEnd.className = 'form-input text-sm';
                    admissionEndInput.replaceWith(newEnd);
                    admissionEndInput = newEnd;
                } else {
                    admissionEndInput.type = 'month';
                }
                admissionStartInput.placeholder = admissionEndInput.placeholder = 'mm/yyyy';
                admissionStartInput.pattern = admissionEndInput.pattern = '';
            } else if (type === 'year') {
                if (admissionStartInput.tagName.toLowerCase() !== 'select') {
                    const newStart = document.createElement('select');
                    newStart.id = 'admissionStartDate';
                    newStart.className = 'form-input text-sm';
                    admissionStartInput.replaceWith(newStart);
                    admissionStartInput = newStart;
                }
                if (admissionEndInput.tagName.toLowerCase() !== 'select') {
                    const newEnd = document.createElement('select');
                    newEnd.id = 'admissionEndDate';
                    newEnd.className = 'form-input text-sm';
                    admissionEndInput.replaceWith(newEnd);
                    admissionEndInput = newEnd;
                }
                populateYearSelect(admissionStartInput);
                populateYearSelect(admissionEndInput);
                admissionStartInput.firstElementChild.text = 'Chọn năm bắt đầu';
                admissionEndInput.firstElementChild.text = 'Chọn năm kết thúc';
            }
            admissionStartInput.value = admissionEndInput.value = '';
            admissionStartInput.addEventListener('input', updateAdmissionResetBtnState);
            admissionEndInput.addEventListener('input', updateAdmissionResetBtnState);
            updateAdmissionResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: admissionStartDate, end: admissionEndDate } = getDateRange());
            loadPatientAdmissionStats(admissionStartDate, admissionEndDate, 'day');
        }
    });

    treatmentTimeType.addEventListener('change', function () {
        const type = this.value;
        if (!type) {
            treatmentDateRange.style.display = 'none';
            treatmentStartInput.value = treatmentEndInput.value = '';
            updateTreatmentResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: treatmentStartDate, end: treatmentEndDate } = getDateRange());
            loadTreatmentCompletionStats(treatmentStartDate, treatmentEndDate, 'day');
        } else {
            treatmentDateRange.style.display = '';
            // Xóa event cũ
            treatmentStartInput.removeEventListener('input', updateTreatmentResetBtnState);
            treatmentEndInput.removeEventListener('input', updateTreatmentResetBtnState);
            // Thay input phù hợp
            if (type === 'day') {
                if (treatmentStartInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'date';
                    newStart.id = 'treatmentStartDate';
                    newStart.className = 'form-input text-sm';
                    treatmentStartInput.replaceWith(newStart);
                    treatmentStartInput = newStart;
                } else {
                    treatmentStartInput.type = 'date';
                }
                if (treatmentEndInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'date';
                    newEnd.id = 'treatmentEndDate';
                    newEnd.className = 'form-input text-sm';
                    treatmentEndInput.replaceWith(newEnd);
                    treatmentEndInput = newEnd;
                } else {
                    treatmentEndInput.type = 'date';
                }
                treatmentStartInput.placeholder = treatmentEndInput.placeholder = 'dd/mm/yyyy';
                treatmentStartInput.pattern = treatmentEndInput.pattern = '';
            } else if (type === 'month') {
                if (treatmentStartInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'month';
                    newStart.id = 'treatmentStartDate';
                    newStart.className = 'form-input text-sm';
                    treatmentStartInput.replaceWith(newStart);
                    treatmentStartInput = newStart;
                } else {
                    treatmentStartInput.type = 'month';
                }
                if (treatmentEndInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'month';
                    newEnd.id = 'treatmentEndDate';
                    newEnd.className = 'form-input text-sm';
                    treatmentEndInput.replaceWith(newEnd);
                    treatmentEndInput = newEnd;
                } else {
                    treatmentEndInput.type = 'month';
                }
                treatmentStartInput.placeholder = treatmentEndInput.placeholder = 'mm/yyyy';
                treatmentStartInput.pattern = treatmentEndInput.pattern = '';
            } else if (type === 'year') {
                if (treatmentStartInput.tagName.toLowerCase() !== 'select') {
                    const newStart = document.createElement('select');
                    newStart.id = 'treatmentStartDate';
                    newStart.className = 'form-input text-sm';
                    treatmentStartInput.replaceWith(newStart);
                    treatmentStartInput = newStart;
                }
                if (treatmentEndInput.tagName.toLowerCase() !== 'select') {
                    const newEnd = document.createElement('select');
                    newEnd.id = 'treatmentEndDate';
                    newEnd.className = 'form-input text-sm';
                    treatmentEndInput.replaceWith(newEnd);
                    treatmentEndInput = newEnd;
                }
                populateYearSelect(treatmentStartInput);
                populateYearSelect(treatmentEndInput);
                treatmentStartInput.firstElementChild.text = 'Chọn năm bắt đầu';
                treatmentEndInput.firstElementChild.text = 'Chọn năm kết thúc';
            }
            treatmentStartInput.value = treatmentEndInput.value = '';
            treatmentStartInput.addEventListener('input', updateTreatmentResetBtnState);
            treatmentEndInput.addEventListener('input', updateTreatmentResetBtnState);
            updateTreatmentResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: treatmentStartDate, end: treatmentEndDate } = getDateRange());
            loadTreatmentCompletionStats(treatmentStartDate, treatmentEndDate, 'day');
            loadSuspendedReasonStats(treatmentStartDate, treatmentEndDate, 'day');
        }
    });

    patientTypeTimeType.addEventListener('change', function () {
        const type = this.value;
        if (!type) {
            patientTypeDateRange.style.display = 'none';
            patientTypeStartInput.value = patientTypeEndInput.value = '';
            updatePatientTypeResetBtnState();
            loadPatientTypeStats();
        } else {
            patientTypeDateRange.style.display = '';
            patientTypeStartInput.removeEventListener('input', updatePatientTypeResetBtnState);
            patientTypeEndInput.removeEventListener('input', updatePatientTypeResetBtnState);
            if (type === 'day') {
                if (patientTypeStartInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'date';
                    newStart.id = 'patientTypeStartDate';
                    newStart.className = 'form-input text-sm';
                    patientTypeStartInput.replaceWith(newStart);
                    patientTypeStartInput = newStart;
                } else {
                    patientTypeStartInput.type = 'date';
                }
                if (patientTypeEndInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'date';
                    newEnd.id = 'patientTypeEndDate';
                    newEnd.className = 'form-input text-sm';
                    patientTypeEndInput.replaceWith(newEnd);
                    patientTypeEndInput = newEnd;
                } else {
                    patientTypeEndInput.type = 'date';
                }
                patientTypeStartInput.placeholder = patientTypeEndInput.placeholder = 'dd/mm/yyyy';
                patientTypeStartInput.pattern = patientTypeEndInput.pattern = '';
            } else if (type === 'month') {
                if (patientTypeStartInput.tagName.toLowerCase() === 'select') {
                    const newStart = document.createElement('input');
                    newStart.type = 'month';
                    newStart.id = 'patientTypeStartDate';
                    newStart.className = 'form-input text-sm';
                    patientTypeStartInput.replaceWith(newStart);
                    patientTypeStartInput = newStart;
                } else {
                    patientTypeStartInput.type = 'month';
                }
                if (patientTypeEndInput.tagName.toLowerCase() === 'select') {
                    const newEnd = document.createElement('input');
                    newEnd.type = 'month';
                    newEnd.id = 'patientTypeEndDate';
                    newEnd.className = 'form-input text-sm';
                    patientTypeEndInput.replaceWith(newEnd);
                    patientTypeEndInput = newEnd;
                } else {
                    patientTypeEndInput.type = 'month';
                }
                patientTypeStartInput.placeholder = patientTypeEndInput.placeholder = 'mm/yyyy';
                patientTypeStartInput.pattern = patientTypeEndInput.pattern = '';
            } else if (type === 'year') {
                if (patientTypeStartInput.tagName.toLowerCase() !== 'select') {
                    const newStart = document.createElement('select');
                    newStart.id = 'patientTypeStartDate';
                    newStart.className = 'form-input text-sm';
                    patientTypeStartInput.replaceWith(newStart);
                    patientTypeStartInput = newStart;
                }
                if (patientTypeEndInput.tagName.toLowerCase() !== 'select') {
                    const newEnd = document.createElement('select');
                    newEnd.id = 'patientTypeEndDate';
                    newEnd.className = 'form-input text-sm';
                    patientTypeEndInput.replaceWith(newEnd);
                    patientTypeEndInput = newEnd;
                }
                populateYearSelect(patientTypeStartInput);
                populateYearSelect(patientTypeEndInput);
                patientTypeStartInput.firstElementChild.text = 'Chọn năm bắt đầu';
                patientTypeEndInput.firstElementChild.text = 'Chọn năm kết thúc';
            }
            patientTypeStartInput.value = patientTypeEndInput.value = '';
            patientTypeStartInput.addEventListener('input', updatePatientTypeResetBtnState);
            patientTypeEndInput.addEventListener('input', updatePatientTypeResetBtnState);
            updatePatientTypeResetBtnState();
            loadPatientTypeStats();
        }
    });

    revenueTimeType.addEventListener('change', function () {
        const type = this.value;
        if (!type) {
            revenueDateRange.style.display = 'none';
            revenueStartInput.value = revenueEndInput.value = '';
            updateRevenueResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: revenueStartDate, end: revenueEndDate } = getDateRange());
            loadRevenueStats(revenueStartDate, revenueEndDate, 'day');
        } else {
            revenueDateRange.style.display = '';
            // Thay input phù hợp
            if (type === 'day') {
                revenueStartInput.type = 'date';
                revenueEndInput.type = 'date';
            } else if (type === 'month') {
                revenueStartInput.type = 'month';
                revenueEndInput.type = 'month';
            } else if (type === 'year') {
                // Replace with select for year
                const newStart = document.createElement('select');
                newStart.id = 'revenueStartDate';
                newStart.className = 'form-input text-sm';
                revenueStartInput.replaceWith(newStart);
                revenueStartInput = newStart;
                const newEnd = document.createElement('select');
                newEnd.id = 'revenueEndDate';
                newEnd.className = 'form-input text-sm';
                revenueEndInput.replaceWith(newEnd);
                revenueEndInput = newEnd;
                // Populate years
                const currentYear = new Date().getFullYear();
                newStart.innerHTML = '<option value=""></option>';
                newEnd.innerHTML = '<option value=""></option>';
                for (let y = currentYear; y >= 2000; y--) {
                    newStart.innerHTML += `<option value="${y}">${y}</option>`;
                    newEnd.innerHTML += `<option value="${y}">${y}</option>`;
                }
                newStart.firstElementChild.text = 'Chọn năm bắt đầu';
                newEnd.firstElementChild.text = 'Chọn năm kết thúc';
            }
            revenueStartInput.addEventListener('change', updateRevenueResetBtnState);
            revenueEndInput.addEventListener('change', updateRevenueResetBtnState);            
            revenueStartInput.value = revenueEndInput.value = '';
            updateRevenueResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: revenueStartDate, end: revenueEndDate } = getDateRange());
            loadRevenueStats(revenueStartDate, revenueEndDate, 'day');
        }
    });

    unpaidPaymentAmountTimeType.addEventListener('change', function () {
        const type = this.value;
        if (!type) {
            unpaidPaymentAmountDateRange.style.display = 'none';
            unpaidPaymentAmountStartInput.value = unpaidPaymentAmountEndInput.value = '';
            updateUnpaidPaymentAmountResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: unpaidPaymentAmountStartDate, end: unpaidPaymentAmountEndDate } = getDateRange());
            loadUnpaidPaymentAmountStats(unpaidPaymentAmountStartDate, unpaidPaymentAmountEndDate, 'day');
        } else {
            unpaidPaymentAmountDateRange.style.display = '';
            // Thay input phù hợp
            if (type === 'day') {
                unpaidPaymentAmountStartInput.type = 'date';
                unpaidPaymentAmountEndInput.type = 'date';
            } else if (type === 'month') {
                unpaidPaymentAmountStartInput.type = 'month';
                unpaidPaymentAmountEndInput.type = 'month';
            } else if (type === 'year') {
                // Replace with select for year
                const newStart = document.createElement('select');
                newStart.id = 'unpaidPaymentAmountStartDate';
                newStart.className = 'form-input text-sm';
                unpaidPaymentAmountStartInput.replaceWith(newStart);
                unpaidPaymentAmountStartInput = newStart;
                const newEnd = document.createElement('select');
                newEnd.id = 'unpaidPaymentAmountEndDate';
                newEnd.className = 'form-input text-sm';
                unpaidPaymentAmountEndInput.replaceWith(newEnd);
                unpaidPaymentAmountEndInput = newEnd;
                // Populate years
                const currentYear = new Date().getFullYear();
                newStart.innerHTML = '<option value=""></option>';
                newEnd.innerHTML = '<option value=""></option>';
                for (let y = currentYear; y >= 2000; y--) {
                    newStart.innerHTML += `<option value="${y}">${y}</option>`;
                    newEnd.innerHTML += `<option value="${y}">${y}</option>`;
                }
                newStart.firstElementChild.text = 'Chọn năm bắt đầu';
                newEnd.firstElementChild.text = 'Chọn năm kết thúc';
            }
            unpaidPaymentAmountStartInput.addEventListener('change', updateUnpaidPaymentAmountResetBtnState);
            unpaidPaymentAmountEndInput.addEventListener('change', updateUnpaidPaymentAmountResetBtnState);            
            unpaidPaymentAmountStartInput.value = unpaidPaymentAmountEndInput.value = '';
            updateUnpaidPaymentAmountResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: unpaidPaymentAmountStartDate, end: unpaidPaymentAmountEndDate } = getDateRange());
            loadUnpaidPaymentAmountStats(unpaidPaymentAmountStartDate, unpaidPaymentAmountEndDate, 'day');
        }
    });

    unpaidPaymentTimeType.addEventListener('change', function () {
        const type = this.value;
        if (!type) {
            unpaidPaymentDateRange.style.display = 'none';
            unpaidPaymentStartInput.value = unpaidPaymentEndInput.value = '';
            updateUnpaidPaymentResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: unpaidPaymentStartDate, end: unpaidPaymentEndDate } = getDateRange());
            loadUnpaidPaymentAmountStats(unpaidPaymentStartDate, unpaidPaymentEndDate, 'day');
        } else {
            unpaidPaymentDateRange.style.display = '';
            // Thay input phù hợp
            if (type === 'day') {
                unpaidPaymentStartInput.type = 'date';
                unpaidPaymentEndInput.type = 'date';
            } else if (type === 'month') {
                unpaidPaymentStartInput.type = 'month';
                unpaidPaymentEndInput.type = 'month';
            } else if (type === 'year') {
                // Replace with select for year
                const newStart = document.createElement('select');
                newStart.id = 'unpaidPaymentStartDate';
                newStart.className = 'form-input text-sm';
                unpaidPaymentStartInput.replaceWith(newStart);
                unpaidPaymentStartInput = newStart;
                const newEnd = document.createElement('select');
                newEnd.id = 'unpaidPaymentEndDate';
                newEnd.className = 'form-input text-sm';
                unpaidPaymentEndInput.replaceWith(newEnd);
                unpaidPaymentEndInput = newEnd;
                // Populate years
                const currentYear = new Date().getFullYear();
                newStart.innerHTML = '<option value=""></option>';
                newEnd.innerHTML = '<option value=""></option>';
                for (let y = currentYear; y >= 2000; y--) {
                    newStart.innerHTML += `<option value="${y}">${y}</option>`;
                    newEnd.innerHTML += `<option value="${y}">${y}</option>`;
                }
                newStart.firstElementChild.text = 'Chọn năm bắt đầu';
                newEnd.firstElementChild.text = 'Chọn năm kết thúc';
            }
            unpaidPaymentStartInput.addEventListener('change', updateUnpaidPaymentResetBtnState);
            unpaidPaymentEndInput.addEventListener('change', updateUnpaidPaymentResetBtnState);            
            unpaidPaymentStartInput.value = unpaidPaymentEndInput.value = '';
            updateUnpaidPaymentResetBtnState();
            // Reset chart về mặc định: 7 ngày gần nhất
            ({ start: unpaidPaymentStartDate, end: unpaidPaymentEndDate } = getDateRange());
            loadUnpaidPaymentAmountStats(unpaidPaymentStartDate, unpaidPaymentEndDate, 'day');
        }
    });
    // Filter button logic
    document.getElementById('admissionFilterBtn').addEventListener('click', function () {
        const timeType = admissionTimeType.value;
        let startDate = admissionStartInput.value;
        let endDate = admissionEndInput.value;

        if (!timeType || !startDate || !endDate) {
            notyf.error('Vui lòng cần chọn khoảng thời gian để lọc');
            return;
        }

        if (timeType === 'month') {
            startDate = startDate ? `${startDate}-01` : '';
            if (endDate) {
                const [year, month] = endDate.split('-');
                const lastDay = new Date(year, month, 0).getDate();
                endDate = `${endDate}-${lastDay}`;
            }
        } else if (timeType === 'year') {
            startDate = startDate ? `${startDate}-01-01` : '';
            endDate = endDate ? `${endDate}-12-31` : '';
        }

        if (timeType === 'day' || timeType === 'month') {
            if (admissionStartInput.value >= admissionEndInput.value) {
                notyf.error(timeType === 'day' ? 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc!' : 'Tháng bắt đầu phải nhỏ hơn tháng kết thúc!');
                return;
            }
        } else if (timeType === 'year') {
            if (parseInt(admissionStartInput.value) >= parseInt(admissionEndInput.value)) {
                notyf.error('Năm bắt đầu phải nhỏ hơn năm kết thúc!');
                return;
            }
        }

        let groupBy = timeType;
        let sDate, eDate;
        if (timeType === 'day' || timeType === 'month') {
            sDate = new Date(admissionStartInput.value);
            eDate = new Date(admissionEndInput.value);
        } else if (timeType === 'year') {
            sDate = new Date(`${admissionStartInput.value}-01-01`);
            eDate = new Date(`${admissionEndInput.value}-12-31`);
        }
        loadPatientAdmissionStats(sDate, eDate, groupBy);
    });

    document.getElementById('departmentFilterBtn').addEventListener('click', function () {
        const timeType = timeTypeSelect.value;
        let startDate = startInput.value;
        let endDate = endInput.value;

        if (!timeType || !startDate || !endDate) {
            notyf.error('Vui lòng cần chọn khoảng thời gian để lọc');
            return;
        }

        if (timeType === 'month') {
            startDate = startDate ? `${startDate}-01` : '';
            if (endDate) {
                const [year, month] = endDate.split('-');
                const lastDay = new Date(year, month, 0).getDate();
                endDate = `${endDate}-${lastDay}`;
            }
        } else if (timeType === 'year') {
            startDate = startDate ? `${startDate}-01-01` : '';
            endDate = endDate ? `${endDate}-12-31` : '';
        }

        if (timeType === 'day' || timeType === 'month') {
            if (startInput.value >= endInput.value) {
                notyf.error(timeType === 'day' ? 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc!' : 'Tháng bắt đầu phải nhỏ hơn tháng kết thúc!');
                return;
            }
        } else if (timeType === 'year') {
            if (parseInt(startInput.value) >= parseInt(endInput.value)) {
                notyf.error('Năm bắt đầu phải nhỏ hơn năm kết thúc!');
                return;
            }
        }

        createDepartmentChart('ccdsChart', 'F75HS667', startDate, endDate);
        createDepartmentChart('vltlChart', '42H35AXU', startDate, endDate);
        createDepartmentChart('bnctChart', 'HZWIPN7U', startDate, endDate);
    });

    document.getElementById('treatmentFilterBtn').addEventListener('click', function () {
        const timeType = treatmentTimeType.value;
        let startDate = treatmentStartInput.value;
        let endDate = treatmentEndInput.value;

        if (!timeType || !startDate || !endDate) {
            notyf.error('Vui lòng cần chọn khoảng thời gian để lọc');
            return;
        }

        if (timeType === 'month') {
            startDate = startDate ? `${startDate}-01` : '';
            if (endDate) {
                const [year, month] = endDate.split('-');
                const lastDay = new Date(year, month, 0).getDate();
                endDate = `${endDate}-${lastDay}`;
            }
        } else if (timeType === 'year') {
            startDate = startDate ? `${startDate}-01-01` : '';
            endDate = endDate ? `${endDate}-12-31` : '';
        }

        if (timeType === 'day' || timeType === 'month') {
            if (treatmentStartInput.value >= treatmentEndInput.value) {
                notyf.error(timeType === 'day' ? 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc!' : 'Tháng bắt đầu phải nhỏ hơn tháng kết thúc!');
                return;
            }
        } else if (timeType === 'year') {
            if (parseInt(treatmentStartInput.value) >= parseInt(treatmentEndInput.value)) {
                notyf.error('Năm bắt đầu phải nhỏ hơn năm kết thúc!');
                return;
            }
        }

        let groupBy = timeType;
        let sDate, eDate;
        if (timeType === 'day' || timeType === 'month') {
            sDate = new Date(treatmentStartInput.value);
            eDate = new Date(treatmentEndInput.value);
        } else if (timeType === 'year') {
            sDate = new Date(`${treatmentStartInput.value}-01-01`);
            eDate = new Date(`${treatmentEndInput.value}-12-31`);
        }
        loadTreatmentCompletionStats(sDate, eDate, groupBy);
        loadSuspendedReasonStats(sDate, eDate, groupBy);
    });

    document.getElementById('patientTypeFilterBtn').addEventListener('click', function () {
        const timeType = patientTypeTimeType.value;
        let startDate = patientTypeStartInput.value;
        let endDate = patientTypeEndInput.value;
        if (!timeType || !startDate || !endDate) {
            notyf.error('Vui lòng cần chọn khoảng thời gian để lọc');
            return;
        }
        if (timeType === 'month') {
            startDate = startDate ? `${startDate}-01` : '';
            if (endDate) {
                const [year, month] = endDate.split('-');
                const lastDay = new Date(year, month, 0).getDate();
                endDate = `${endDate}-${lastDay}`;
            }
        } else if (timeType === 'year') {
            startDate = startDate ? `${startDate}-01-01` : '';
            endDate = endDate ? `${endDate}-12-31` : '';
        }
        if (timeType === 'day' || timeType === 'month') {
            if (patientTypeStartInput.value >= patientTypeEndInput.value) {
                notyf.error(timeType === 'day' ? 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc!' : 'Tháng bắt đầu phải nhỏ hơn tháng kết thúc!');
                return;
            }
        } else if (timeType === 'year') {
            if (parseInt(patientTypeStartInput.value) >= parseInt(patientTypeEndInput.value)) {
                notyf.error('Năm bắt đầu phải nhỏ hơn năm kết thúc!');
                return;
            }
        }
        let groupBy = timeType;
        let sDate, eDate;
        if (timeType === 'day' || timeType === 'month') {
            sDate = new Date(patientTypeStartInput.value);
            eDate = new Date(patientTypeEndInput.value);
        } else if (timeType === 'year') {
            sDate = new Date(`${patientTypeStartInput.value}-01-01`);
            eDate = new Date(`${patientTypeEndInput.value}-12-31`);
        }
        loadPatientTypeStats(sDate, eDate, groupBy);
    });

    document.getElementById('revenueFilterBtn').addEventListener('click', function () {
        const timeType = revenueTimeType.value;
        let startDate = revenueStartInput.value;
        let endDate = revenueEndInput.value;
    
        if (!timeType || !startDate || !endDate) {
            notyf.error('Vui lòng cần chọn khoảng thời gian để lọc');
            return;
        }
    
        if (timeType === 'month') {
            startDate = startDate ? `${startDate}-01` : '';
            if (endDate) {
                const [year, month] = endDate.split('-');
                const lastDay = new Date(year, month, 0).getDate();
                endDate = `${endDate}-${lastDay}`;
            }
        } else if (timeType === 'year') {
            startDate = startDate ? `${startDate}-01-01` : '';
            endDate = endDate ? `${endDate}-12-31` : '';
        }
    
        if (timeType === 'day' || timeType === 'month') {
            if (revenueStartInput.value >= revenueEndInput.value) {
                notyf.error(timeType === 'day' ? 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc!' : 'Tháng bắt đầu phải nhỏ hơn tháng kết thúc!');
                return;
            }
        } else if (timeType === 'year') {
            if (parseInt(revenueStartInput.value) >= parseInt(revenueEndInput.value)) {
                notyf.error('Năm bắt đầu phải nhỏ hơn năm kết thúc!');
                return;
            }
        }
    
        let groupBy = timeType;
        let sDate, eDate;
        if (timeType === 'day' || timeType === 'month') {
            sDate = new Date(revenueStartInput.value);
            eDate = new Date(revenueEndInput.value);
        } else if (timeType === 'year') {
            sDate = new Date(`${revenueStartInput.value}-01-01`);
            eDate = new Date(`${revenueEndInput.value}-12-31`);
        }
        loadRevenueStats(sDate, eDate, groupBy);
    });

    document.getElementById('unpaidPaymentFilterBtn').addEventListener('click', function () {
        const timeType = unpaidPaymentTimeType.value;
        let startDate = unpaidPaymentStartInput.value;
        let endDate = unpaidPaymentEndInput.value;
    
        if (!timeType || !startDate || !endDate) {
            notyf.error('Vui lòng cần chọn khoảng thời gian để lọc');
            return;
        }
    
        if (timeType === 'month') {
            startDate = startDate ? `${startDate}-01` : '';
            if (endDate) {
                const [year, month] = endDate.split('-');
                const lastDay = new Date(year, month, 0).getDate();
                endDate = `${endDate}-${lastDay}`;
            }
        } else if (timeType === 'year') {
            startDate = startDate ? `${startDate}-01-01` : '';
            endDate = endDate ? `${endDate}-12-31` : '';
        }
    
        if (timeType === 'day' || timeType === 'month') {
            if (unpaidPaymentStartInput.value >= unpaidPaymentEndInput.value) {
                notyf.error(timeType === 'day' ? 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc!' : 'Tháng bắt đầu phải nhỏ hơn tháng kết thúc!');
                return;
            }
        } else if (timeType === 'year') {
            if (parseInt(unpaidPaymentStartInput.value) >= parseInt(unpaidPaymentEndInput.value)) {
                notyf.error('Năm bắt đầu phải nhỏ hơn năm kết thúc!');
                return;
            }
        }
    
        let groupBy = timeType;
        let sDate, eDate;
        if (timeType === 'day' || timeType === 'month') {
            sDate = new Date(unpaidPaymentStartInput.value);
            eDate = new Date(unpaidPaymentEndInput.value);
        } else if (timeType === 'year') {
            sDate = new Date(`${unpaidPaymentStartInput.value}-01-01`);
            eDate = new Date(`${unpaidPaymentEndInput.value}-12-31`);
        }
        loadUnpaidPaymentCountStats(sDate, eDate, groupBy);
    });

    document.getElementById('unpaidPaymentAmountFilterBtn').addEventListener('click', function () {
        const timeType = unpaidPaymentAmountTimeType.value;
        let startDate = unpaidPaymentAmountStartInput.value;
        let endDate = unpaidPaymentAmountEndInput.value;
    
        if (!timeType || !startDate || !endDate) {
            notyf.error('Vui lòng cần chọn khoảng thời gian để lọc');
            return;
        }
    
        if (timeType === 'month') {
            startDate = startDate ? `${startDate}-01` : '';
            if (endDate) {
                const [year, month] = endDate.split('-');
                const lastDay = new Date(year, month, 0).getDate();
                endDate = `${endDate}-${lastDay}`;
            }
        } else if (timeType === 'year') {
            startDate = startDate ? `${startDate}-01-01` : '';
            endDate = endDate ? `${endDate}-12-31` : '';
        }
    
        if (timeType === 'day' || timeType === 'month') {
            if (unpaidPaymentAmountStartInput.value >= unpaidPaymentAmountEndInput.value) {
                notyf.error(timeType === 'day' ? 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc!' : 'Tháng bắt đầu phải nhỏ hơn tháng kết thúc!');
                return;
            }
        } else if (timeType === 'year') {
            if (parseInt(unpaidPaymentAmountStartInput.value) >= parseInt(unpaidPaymentAmountEndInput.value)) {
                notyf.error('Năm bắt đầu phải nhỏ hơn năm kết thúc!');
                return;
            }
        }
    
        let groupBy = timeType;
        let sDate, eDate;
        if (timeType === 'day' || timeType === 'month') {
            sDate = new Date(unpaidPaymentAmountStartInput.value);
            eDate = new Date(unpaidPaymentAmountEndInput.value);
        } else if (timeType === 'year') {
            sDate = new Date(`${unpaidPaymentAmountStartInput.value}-01-01`);
            eDate = new Date(`${unpaidPaymentAmountEndInput.value}-12-31`);
        }
        loadUnpaidPaymentAmountStats(sDate, eDate, groupBy);
    });

    // Reset button logic
    admissionResetBtn.addEventListener('click', function () {
        admissionTimeType.value = '';
        admissionDateRange.style.display = 'none';
        admissionStartInput.value = admissionEndInput.value = '';
        if (admissionStartInput.tagName.toLowerCase() !== 'input') {
            const newStart = document.createElement('input');
            newStart.type = 'date';
            newStart.id = 'admissionStartDate';
            newStart.className = 'form-input text-sm';
            admissionStartInput.replaceWith(newStart);
            admissionStartInput = newStart;
        } else {
            admissionStartInput.type = 'date';
        }
        if (admissionEndInput.tagName.toLowerCase() !== 'input') {
            const newEnd = document.createElement('input');
            newEnd.type = 'date';
            newEnd.id = 'admissionEndDate';
            newEnd.className = 'form-input text-sm';
            admissionEndInput.replaceWith(newEnd);
            admissionEndInput = newEnd;
        } else {
            admissionEndInput.type = 'date';
        }
        updateAdmissionResetBtnState();
        const existingChart = document.querySelector('#patientAdmissionChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }
        // Khởi tạo mặc định cho filter admission: 7 ngày gần nhất
        ({ start: admissionStartDate, end: admissionEndDate } = getDateRange());
        loadPatientAdmissionStats(admissionStartDate, admissionEndDate, 'day');
    });

    treatmentResetBtn.addEventListener('click', function () {
        treatmentTimeType.value = '';
        treatmentDateRange.style.display = 'none';
        treatmentStartInput.value = treatmentEndInput.value = '';
        if (treatmentStartInput.tagName.toLowerCase() !== 'input') {
            const newStart = document.createElement('input');
            newStart.type = 'date';
            newStart.id = 'treatmentStartDate';
            newStart.className = 'form-input text-sm';
            treatmentStartInput.replaceWith(newStart);
            treatmentStartInput = newStart;
        } else {
            treatmentStartInput.type = 'date';
        }
        if (treatmentEndInput.tagName.toLowerCase() !== 'input') {
            const newEnd = document.createElement('input');
            newEnd.type = 'date';
            newEnd.id = 'treatmentEndDate';
            newEnd.className = 'form-input text-sm';
            treatmentEndInput.replaceWith(newEnd);
            treatmentEndInput = newEnd;
        } else {
            treatmentEndInput.type = 'date';
        }
        updateTreatmentResetBtnState();
        // Đảm bảo render lại biểu đồ mặc định
        ({ start: treatmentStartDate, end: treatmentEndDate } = getDateRange());
        const existingChart = document.querySelector('#treatmentCompletionChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }
        loadTreatmentCompletionStats(treatmentStartDate, treatmentEndDate, 'day');
        loadSuspendedReasonStats(treatmentStartDate, treatmentEndDate, 'day');
    });

    resetBtn.addEventListener('click', function () {
        timeTypeSelect.value = '';
        dateRangeDiv.style.display = 'none';
        startInput.value = endInput.value = '';
        if (startInput.tagName.toLowerCase() !== 'input') {
            const newStart = document.createElement('input');
            newStart.type = 'date';
            newStart.id = 'departmentStartDate';
            newStart.className = 'form-input text-sm';
            startInput.replaceWith(newStart);
            startInput = newStart;
        } else {
            startInput.type = 'date';
        }
        if (endInput.tagName.toLowerCase() !== 'input') {
            const newEnd = document.createElement('input');
            newEnd.type = 'date';
            newEnd.id = 'departmentEndDate';
            newEnd.className = 'form-input text-sm';
            endInput.replaceWith(newEnd);
            endInput = newEnd;
        } else {
            endInput.type = 'date';
        }
        updateResetBtnState();
        // GỌI LẠI CHART MẶC ĐỊNH CHO TỪNG KHOA
        createDepartmentChart('ccdsChart', 'F75HS667');
        createDepartmentChart('vltlChart', '42H35AXU');
        createDepartmentChart('bnctChart', 'HZWIPN7U');
    });

    patientTypeResetBtn.addEventListener('click', function () {
        patientTypeTimeType.value = '';
        patientTypeDateRange.style.display = 'none';
        patientTypeStartInput.value = patientTypeEndInput.value = '';
        patientTypeStartInput.type = 'date';
        patientTypeEndInput.type = 'date';
        updatePatientTypeResetBtnState();
        const existingChart = document.querySelector('#patientTypeChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }
        loadPatientTypeStats();
    });

    revenueResetBtn.addEventListener('click', function () {
        revenueTimeType.value = '';
        revenueDateRange.style.display = 'none';
        revenueStartInput.value = revenueEndInput.value = '';
        if (revenueStartInput.tagName.toLowerCase() !== 'input') {
            const newStart = document.createElement('input');
            newStart.type = 'date';
            newStart.id = 'revenueStartDate';
            newStart.className = 'form-input text-sm';
            revenueStartInput.replaceWith(newStart);
            revenueStartInput = newStart;
        } else {
            revenueStartInput.type = 'date';
        }
        if (revenueEndInput.tagName.toLowerCase() !== 'input') {
            const newEnd = document.createElement('input');
            newEnd.type = 'date';
            newEnd.id = 'revenueEndDate';
            newEnd.className = 'form-input text-sm';
            revenueEndInput.replaceWith(newEnd);
            revenueEndInput = newEnd;
        } else {
            revenueEndInput.type = 'date';
        }
        updateRevenueResetBtnState();
        // Khởi tạo mặc định: 7 ngày gần nhất
        ({ start: revenueStartDate, end: revenueEndDate } = getDateRange());
        loadRevenueStats(revenueStartDate, revenueEndDate, 'day');
    });

    unpaidPaymentAmountResetBtn.addEventListener('click', function () {
        unpaidPaymentAmountTimeType.value = '';
        unpaidPaymentAmountDateRange.style.display = 'none';
        unpaidPaymentAmountStartInput.value = unpaidPaymentAmountEndInput.value = '';
        if (unpaidPaymentAmountStartInput.tagName.toLowerCase() !== 'input') {
            const newStart = document.createElement('input');
            newStart.type = 'date';
            newStart.id = 'unpaidPaymentAmountStartDate';
            newStart.className = 'form-input text-sm';
            unpaidPaymentAmountStartInput.replaceWith(newStart);
            unpaidPaymentAmountStartInput = newStart;
        } else {
            unpaidPaymentAmountStartInput.type = 'date';
        }
        if (unpaidPaymentAmountEndInput.tagName.toLowerCase() !== 'input') {
            const newEnd = document.createElement('input');
            newEnd.type = 'date';
            newEnd.id = 'unpaidPaymentAmountEndDate';
            newEnd.className = 'form-input text-sm';
            unpaidPaymentAmountEndInput.replaceWith(newEnd);
            unpaidPaymentAmountEndInput = newEnd;
        } else {
            unpaidPaymentAmountEndInput.type = 'date';
        }
        updateUnpaidPaymentAmountResetBtnState();
        const existingChart = document.querySelector('#unpaidPaymentAmountChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }
        // Khởi tạo mặc định: 7 ngày gần nhất
        ({ start: unpaidPaymentAmountStartDate, end: unpaidPaymentAmountEndDate } = getDateRange());
        loadUnpaidPaymentAmountStats(unpaidPaymentAmountStartDate, unpaidPaymentAmountEndDate, 'day');
    });

    unpaidPaymentResetBtn.addEventListener('click', function () {
        unpaidPaymentTimeType.value = '';
        unpaidPaymentDateRange.style.display = 'none';
        unpaidPaymentStartInput.value = unpaidPaymentEndInput.value = '';
        if (unpaidPaymentStartInput.tagName.toLowerCase() !== 'input') {
            const newStart = document.createElement('input');
            newStart.type = 'date';
            newStart.id = 'unpaidPaymentStartDate';
            newStart.className = 'form-input text-sm';
            unpaidPaymentStartInput.replaceWith(newStart);
            unpaidPaymentStartInput = newStart;
        } else {
            unpaidPaymentStartInput.type = 'date';
        }
        if (unpaidPaymentEndInput.tagName.toLowerCase() !== 'input') {
            const newEnd = document.createElement('input');
            newEnd.type = 'date';
            newEnd.id = 'unpaidPaymentEndDate';
            newEnd.className = 'form-input text-sm';
            unpaidPaymentEndInput.replaceWith(newEnd);
            unpaidPaymentEndInput = newEnd;
        } else {
            unpaidPaymentEndInput.type = 'date';
        }
        updateUnpaidPaymentResetBtnState();
        const existingChart = document.querySelector('#unpaidPaymentChart');
        if (existingChart.__chartInstance) {
            existingChart.__chartInstance.destroy();
        }
        // Khởi tạo mặc định: 7 ngày gần nhất
        ({ start: unpaidPaymentStartDate, end: unpaidPaymentEndDate } = getDateRange());
        loadUnpaidPaymentCountStats(unpaidPaymentStartDate, unpaidPaymentEndDate, 'day');
    });

    // Function to load and display all statistics
    async function loadAllStats() {
        const departments = [
            {
                id: 'F75HS667',
                chartId: 'ccdsChart'
            },
            {
                id: '42H35AXU',
                chartId: 'vltlChart'
            },
            {
                id: 'HZWIPN7U',
                chartId: 'bnctChart'
            }
        ];

        // Lấy 7 ngày: hôm nay ở giữa
        const today = new Date();
        ({ start: admissionStartDate, end: admissionEndDate } = getDateRange());
        await loadPatientAdmissionStats(admissionStartDate, admissionEndDate, 'day');

        // Load treatment completion stats: 7 ngày (hôm nay ở giữa)
        ({ start: treatmentStartDate, end: treatmentEndDate } = getDateRange());
        await loadTreatmentCompletionStats(treatmentStartDate, treatmentEndDate, 'day');
        await loadSuspendedReasonStats();

        // Load revenue stats: 7 ngày (hôm nay ở giữa)
        ({ start: revenueStartDate, end: revenueEndDate } = getDateRange());
        await loadRevenueStats(revenueStartDate, revenueEndDate, 'day');

        // Load unpaid payment count stats: 7 ngày (hôm nay ở giữa)
        ({ start: unpaidPaymentStartDate, end: unpaidPaymentEndDate } = getDateRange());
        await loadUnpaidPaymentCountStats(unpaidPaymentStartDate, unpaidPaymentEndDate, 'day');

        // Load unpaid payment amount stats: 7 ngày (hôm nay ở giữa)
        ({ start: unpaidPaymentAmountStartDate, end: unpaidPaymentAmountEndDate } = getDateRange());
        await loadUnpaidPaymentAmountStats(unpaidPaymentAmountStartDate, unpaidPaymentAmountEndDate, 'day');

        // Load department stats
        for (const dept of departments) {
            await createDepartmentChart(dept.chartId, dept.id);
        }

        await loadPatientTypeStats();
    }
});


function getDateRange(centerDate = new Date(), before = 3, after = 3) {
    const start = new Date(centerDate);
    start.setDate(centerDate.getDate() - before);
    const end = new Date(centerDate);
    end.setDate(centerDate.getDate() + after);
    return { start, end };
}