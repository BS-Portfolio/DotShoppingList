<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const username = ref('');
const email = ref('');
const password = ref('');

const saveToSession = (key: string, value: any) => localStorage.setItem(key, JSON.stringify(value));

const handleLoginSuccess = () => {
  saveToSession('isAuthenticated', true);
  authStore.isAuthenticated = true;
  router.push('/');
};

const validateEmail = (email: string): boolean => {
  const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return re.test(email);
};

const register = () => {
  if (!username.value || !email.value || !password.value) {
    alert('Bitte füllen Sie alle Felder aus.');
    return;
  }

  if (!validateEmail(email.value)) {
    alert('Bitte geben Sie eine gültige E-Mail-Adresse ein.');
    return;
  }

  const encodedUsername = btoa(username.value);
  const encodedEmail = btoa(email.value);
  const encodedPassword = btoa(password.value);

  console.log("Username (Base64):", encodedUsername);
  console.log("Email (Base64):", encodedEmail);
  console.log("Password (Base64):", encodedPassword);

  authStore.register(username.value, email.value, password.value);
  if (authStore.isAuthenticated) handleLoginSuccess();
};
</script>

<template>
  <div class="login">
    <div class="card">
      <h1 class="caveat-brush-regular">Registrieren</h1>
      <input v-model="email" type="email" placeholder="Email" />
      <input v-model="username" placeholder="Username" />
      <input v-model="password" type="password" placeholder="Password" @keyup.enter="register" />
      <button @click="register">Registrieren</button>
      <RouterLink to="/login" class="login-link">Bereits einen Account? Login</RouterLink>
    </div>
  </div>
</template>

<style scoped>
.login {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  height: 100vh;
  margin-top: 2rem;
}

.card {
  padding: 2rem;
  background-color: #fff8dc;
  border-radius: 10px;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  border: 2px solid var(--color-primary);
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 400px;
}

input {
  width: 30%;
  height: 3vh;
  font-size: 18px;
  margin-bottom: 0.5rem;
}

button {
  padding: 10px 20px;
  font-size: 16px;
  margin-top: 0.5rem;
}

.login-link {
  margin-top: 1rem;
  text-decoration: none;
  font-size: 14px;
}
</style>
