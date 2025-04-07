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
      <RouterLink to="/login" class="link">Bereits einen Account? Login</RouterLink>
    </div>
  </div>
</template>

<style scoped>
@import './../assets/auth.css';
</style>
