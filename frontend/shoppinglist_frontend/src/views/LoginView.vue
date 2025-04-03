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
      <h1>Login</h1>
      <input v-model="username" placeholder="Username" />
      <input v-model="password" type="password" placeholder="Password" @keyup.enter="login" />
      <button @click="login">Login</button>
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

h1 {
  color: var(--color-primary);
  margin-top: 0.05rem;
}

input {
  width: 30%;
  height: 5vh;
  font-size: 18px;
  text-align: center;
  border: 2px solid var(--color-primary);
  background: transparent;
  color: var(--color-primary);
  margin-bottom: 1rem;
}

input:focus {
  border-color: var(--color-primary);
  outline: none;
}

button {
  padding: 10px 20px;
  font-size: 16px;
  cursor: pointer;
  margin-top: 2rem;
  background-color: #fff8dc;
  border: 2px solid var(--color-primary);
  color: var(--color-primary);
}
</style>
