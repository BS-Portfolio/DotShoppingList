<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const username = ref('');
const password = ref('');

const saveToSession = (key: string, value: any) => localStorage.setItem(key, JSON.stringify(value));

const handleLoginSuccess = () => {
  saveToSession('isAuthenticated', true);
  authStore.isAuthenticated = true;
  router.push('/');
};

const login = () => {
  authStore.login(username.value, password.value);
  if (authStore.isAuthenticated) handleLoginSuccess();
};
</script>

<template>
  <div class="login">
    <div class="card">
      <h1 class="caveat-brush-regular">Login</h1>
      <input v-model="username" placeholder="Username" />
      <input v-model="password" type="password" placeholder="Password" @keyup.enter="login" />
      <button @click="login">Login</button>
      <RouterLink to="/registration" class="link">Noch keinen Account? Registrieren</RouterLink>
    </div>
  </div>
</template>

<style scoped>
@import './../assets/auth.css';
</style>
