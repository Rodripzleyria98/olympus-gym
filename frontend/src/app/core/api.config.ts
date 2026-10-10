const isLocalHost = typeof window !== 'undefined' && ['localhost', '127.0.0.1'].includes(window.location.hostname);

export const API_BASE_URL = isLocalHost
	? 'http://localhost:5110/api'
	: 'https://olympus-api-fvat.onrender.com/api';