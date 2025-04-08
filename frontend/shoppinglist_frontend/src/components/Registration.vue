<script setup lang="ts">
import { ref } from 'vue';
import { useAuthHelpers } from '@/composables/useAuthHelpers';

const firstname = ref('');
const lastname = ref('');
const username = ref('');
const email = ref('');
const password = ref('');

const { encode, handleLoginSuccess, validateEmail } = useAuthHelpers();

const register = async () => {
  if (!username.value || !email.value || !password.value) {
    alert('Please fill out all fields.');
    return;
  }

  if (!validateEmail(email.value)) {
    alert('Please enter a valid email address.');
    return;
  }

  let response: Response;

  try {
    response = await fetch('https://localhost:7191/ShoppingListApi/User', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        firstname: firstname.value,
        lastname: lastname.value,
        username: username.value,
        emailAddress64: encode(email.value),
        password64: encode(password.value)
      })
    });
  } catch (networkError) {
    alert('A network error occurred. Please try again.');
    return;
  }

  if (response.status === 409) {
    alert('A user with this email already exists. Please use a different email.');
    return;
  }

  if (!response.ok) {
    const errorText = await response.text();
    alert(`Registration failed: ${errorText}`);
    return;
  }

  try {
    await response.json();
    void handleLoginSuccess();
  } catch (error) {
    alert('Registration succeeded, but an error occurred afterward.');
  }
};
</script>


<template>
  <div class="login">
    <div class="card">
      <h1 class="caveat-brush-regular">Register</h1>
      <form @submit.prevent="register">
        <input v-model="firstname" placeholder="First name"/>
        <input v-model="lastname" placeholder="Last name"/>
        <input v-model="email" type="email" placeholder="Email"/>
        <input v-model="username" placeholder="Username"/>
        <input v-model="password" type="password" placeholder="Password"/>
        <button type="submit">Register</button>
      </form>
      <RouterLink to="/login" class="auth-link">Already have an account? Login now.</RouterLink>
    </div>
  </div>
</template>

<style scoped>
@import './../assets/auth.css';
</style>
