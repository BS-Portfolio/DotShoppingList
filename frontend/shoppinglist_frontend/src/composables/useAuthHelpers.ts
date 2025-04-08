import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

export const useAuthHelpers = () => {
  const authStore = useAuthStore();
  const router = useRouter();

  const saveToSession = (key: string, value: any) =>
    localStorage.setItem(key, JSON.stringify(value));

  const encode = (text: string) => btoa(text);

  const handleLoginSuccess = (userData?: any) => {
    saveToSession('isAuthenticated', true);
    if (userData) saveToSession('userData', userData);
    authStore.isAuthenticated = true;
    void router.push('/')
  };

  const validateEmail = (email: string): boolean => {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  };

  return {
    saveToSession,
    encode,
    handleLoginSuccess,
    validateEmail,
  };
};
