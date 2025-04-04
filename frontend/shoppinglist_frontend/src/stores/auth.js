import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
    state: () => ({
        isAuthenticated: false,
    }),
    actions: {
        login(username, password) {
            if (username === 'test' && password === '1234') {
                this.isAuthenticated = true
            } else {
                alert('Ungültiger Benutzername oder Passwort')
            }
        },
        logout() {
            this.isAuthenticated = false
        },
      register() {
        this.isAuthenticated = true;
      },
    },
})
