<script setup lang="ts">
import { ref } from 'vue';
import { useAuthHelpers } from '@/composables/useAuthHelpers';
import { useAuthStore } from '@/stores/auth';

const username = ref('');
const password = ref('');
const authStore = useAuthStore();

const { encode, handleLoginSuccess } = useAuthHelpers();

const login = async () => {
  await authStore.login(username.value, password.value, encode, handleLoginSuccess);
};
</script>

<template>
  <div class="login">
    <div class="card">
      <h1 class="caveat-brush-regular">Login</h1>
      <form @submit.prevent="login">
        <input v-model="username" placeholder="Username" autocomplete="username" />
        <input v-model="password" type="password" placeholder="Password" autocomplete="current-password" />
        <button type="submit">Login</button>
      </form>
      <RouterLink to="/registration" class="auth-link">Don't have an account? Register now.</RouterLink>
    </div>
  </div>
</template>

<style scoped>
@import './../assets/auth.css';
</style>
