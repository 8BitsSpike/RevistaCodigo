export const formatDate = (date: string | Date) => {
    if (!date) return '';
    return new Date(date).toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
};

export const formatDateTime = (date: string | Date) => {
    if (!date) return '';
    return new Date(date).toLocaleString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};

// Helper específico para o PendingCard que separa data e ano
export const formatDayMonth = (date: string | Date) => {
    if (!date) return '';
    return new Date(date).toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit'
    });
};