

<template>
  <v-container
    class="d-flex align-center justify-center"
    fluid
    fill-height
  >
    <v-card class="pa-4" min-width="400">
      <v-card-title class="text-h5">Login</v-card-title>
      <v-card-subtitle class="mb-4">Enter your credentials</v-card-subtitle>

      <v-form>
        <v-text-field
          v-model="email"
          label="Email"
          prepend-icon="mdi-email"
          type="text"
          required
        ></v-text-field>

        <v-text-field
          v-model="password"
          label="Password"
          prepend-icon="mdi-lock"
          type="password"
          required
        ></v-text-field>

        <v-btn
          color="primary"
          @click="login"
          class="mt-4"
        >
          Login
        </v-btn>
        <div v-if="loginError != ''" class="text-center mt-4 mb-2 error--text">{{ loginError }}</div>
      </v-form>
    </v-card>
  </v-container>
</template>

<script>
import axios from '../axios/axios'
export default {
  name:'LoginComponent',
  data() {
    return {
      email: '',
      password: '',
      loginError: ''
    };
  },
  methods: {
    login() {
        axios.post('/user/login',{
            UserName: this.email,
            PassWord: this.password            
        }).then(function (response){
            alert("OK");
            localStorage.setItem('auth', response.data.token)
            location = '/home'
        }).catch((error) => {
            this.loginError = error.response.data.message;
        });
    }
  }
};
</script>

<style scoped>
.v-card {
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}
</style>
