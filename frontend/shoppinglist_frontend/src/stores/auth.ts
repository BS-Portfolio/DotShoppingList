import { defineStore } from 'pinia'

interface AuthState {
  isAuthenticated: boolean;
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    isAuthenticated: false,
  }),

  actions: {
    login(username: string, password: string): void {
      if (username === 'test' && password === '1234') {
        this.isAuthenticated = true;
      } else {
        alert('Ungültiger Benutzername oder Passwort');
      }
    },

    logout(): void {
      this.isAuthenticated = false;
    },

    register(): void {
      this.isAuthenticated = true;
    }
  }
});
