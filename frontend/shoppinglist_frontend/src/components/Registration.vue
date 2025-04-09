<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useAuthHelpers } from '@/composables/useAuthHelpers';

const firstname = ref('');
const lastname = ref('');
const username = ref('');
const email = ref('');
const password = ref('');

const { encode, handleLoginSuccess, validateEmail } = useAuthHelpers();
const authStore = useAuthStore();

const register = async () => {
  await authStore.register(
    firstname.value,
    lastname.value,
    username.value,
    email.value,
    password.value,
    encode,
    handleLoginSuccess,
    validateEmail
  );
};
</script>

<template>
  <div class="login">
    <div class="card">
      <h1 class="caveat-brush-regular">Register</h1>
      <form @submit.prevent="register">
        <input v-model="firstname" placeholder="First name" />
        <input v-model="lastname" placeholder="Last name" />
        <input v-model="email" type="email" placeholder="Email" />
        <input v-model="username" placeholder="Username" />
        <input v-model="password" type="password" placeholder="Password" />
        <button type="submit">Register</button>
      </form>
      <RouterLink to="/login" class="auth-link">Already have an account? Login now.</RouterLink>
    </div>
  </div>
</template>

<style scoped>
@import './../assets/auth.css';
</style>
