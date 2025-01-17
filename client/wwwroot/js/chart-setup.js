function initializeCharts(transactionStats) {
    if (!transactionStats) {
        console.error("Transaction stats are missing!");
        return;
    }

    const transactionSummaryChartCanvas = document.getElementById('transaction-summary-chart');
    if (transactionSummaryChartCanvas) {
        const transactionSummaryChart = new Chart(
            transactionSummaryChartCanvas.getContext('2d'),
            {
                type: 'doughnut',
                data: {
                    labels: ['Inflows', 'Outflows', 'Debt'],
                    datasets: [{
                        label: 'Summary',
                        data: [
                            transactionStats.totalInflows,
                            transactionStats.totalOutflows,
                            transactionStats.totalDebt
                        ],
                        backgroundColor: ['#4CAF50', '#F44336', '#FFC107'],
                    }],
                },
            }
        );
    } else {
        console.error('Error: transaction-summary-chart canvas not found!');
    }

    const transactionStatisticsChartCanvas = document.getElementById('transaction-statistics-chart');
    if (transactionStatisticsChartCanvas) {
        const transactionStatisticsChart = new Chart(
            transactionStatisticsChartCanvas.getContext('2d'),
            {
                type: 'bar',
                data: {
                    labels: ['Inflows', 'Outflows', 'Debt Cleared', 'Remaining Debt'],
                    datasets: [{
                        label: 'Statistics',
                        data: [
                            transactionStats.totalInflows,
                            transactionStats.totalOutflows,
                            transactionStats.clearedDebt,
                            transactionStats.remainingDebt
                        ],
                        backgroundColor: ['#4CAF50', '#F44336', '#FFC107', '#03A9F4'],
                    }],
                },
            }
        );
    } else {
        console.error('Error: transaction-statistics-chart canvas not found!');
    }
}
