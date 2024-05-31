import { get, post } from '../services/apiService';

export async function getCurrentUser() {
    return await get('/api/users/CurrentLogin');
}

export async function findUserByEmailOrLogin(emailOrLogin) {
    return await post('/api/users/findbyemail', { query: emailOrLogin });
}
