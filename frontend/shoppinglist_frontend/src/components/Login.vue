<script setup lang="ts">
import {ref} from 'vue';
import {useAuthStore} from '@/stores/auth';
import {useRouter} from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const username = ref('');
const password = ref('');

const saveToSession = (key: string, value: any) => localStorage.setItem(key, JSON.stringify(value));

const handleLoginSuccess = (responseData: any) => {
  saveToSession('isAuthenticated', true);
  saveToSession('userData', responseData);
  authStore.isAuthenticated = true;
  router.push('/');
};

const login = async () => {
  const encodedEmail = btoa(username.value);
  const encodedPassword = btoa(password.value);

  try {
    const response = await fetch('https://localhost:7191/ShoppingListApi/User/Login', {
      method: 'POST',
      headers: {
        'Accept': 'text/plain',
        'Content-Type': 'application/json',
        'X-Frontend': ''
      },
      body: JSON.stringify({
        emailAddress: encodedEmail,
        password: encodedPassword
      })
    });

    if (response.status === 200) {
      const responseData = await response.json();
      handleLoginSuccess(responseData);
    } else {
      alert('Login failed. Please check your credentials.');
    }
  } catch (error) {
    console.log(error)
    alert('An error occurred during login. Please try again.');
  }
};
</script>

<template>
  <div class="login">
    <div class="card">
      <h1 class="caveat-brush-regular">Login</h1>
      <form @submit.prevent="login">
        <input v-model="username" placeholder="Username" autocomplete="username"/>
        <input v-model="password" type="password" placeholder="Password"
               autocomplete="current-password"/>
        <button type="submit">Login</button>
      </form>
      <RouterLink to="/registration" class="auth-link">Don't have an account? Register now.
      </RouterLink>
    </div>
  </div>
</template>

<style scoped>
@import './../assets/auth.css';
</style>
