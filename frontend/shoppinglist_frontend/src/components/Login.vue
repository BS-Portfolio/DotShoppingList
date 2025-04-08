<script setup lang="ts">
import {ref} from 'vue';
import {useAuthHelpers} from '@/composables/useAuthHelpers';

const username = ref('');
const password = ref('');

const {encode, handleLoginSuccess} = useAuthHelpers();

const login = async () => {
  const encodedEmail = encode(username.value);
  const encodedPassword = encode(password.value);

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
      void handleLoginSuccess(responseData);
    } else {
      alert('Login failed. Please check your credentials.');
    }
  } catch (error) {
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
