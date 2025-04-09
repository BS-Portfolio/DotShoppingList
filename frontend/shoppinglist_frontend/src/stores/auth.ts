import {defineStore} from 'pinia';

interface AuthState {
    isAuthenticated: boolean;
}

const baseUrl = import.meta.env.VITE_API_BASE_URL;

export const useAuthStore = defineStore('auth', {
    state: (): AuthState => ({
        isAuthenticated: false,
    }),

    actions: {
        logout(): void {
            this.isAuthenticated = false;
            localStorage.clear();
        },

        async register(
            firstname: string,
            lastname: string,
            username: string,
            email: string,
            password: string,
            encode: (value: string) => string,
            handleLoginSuccess: () => Promise<void>,
            validateEmail: (email: string) => boolean
        ): Promise<void> {
            if (!username || !email || !password) {
                alert('Please fill out all fields.');
                return;
            }

            if (!validateEmail(email)) {
                alert('Please enter a valid email address.');
                return;
            }

            let response: Response;

            try {
                response = await fetch(`${baseUrl}/User`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        firstname,
                        lastname,
                        username,
                        emailAddress64: encode(email),
                        password64: encode(password),
                    }),
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
                await handleLoginSuccess();
            } catch (error) {
                alert('Registration succeeded, but an error occurred afterward.');
            }
        },

        async login(
            username: string,
            password: string,
            encode: (value: string) => string,
            handleLoginSuccess: (userData?: any) => Promise<void>
        ): Promise<void> {
            const encodedEmail = encode(username);
            const encodedPassword = encode(password);

            try {
                const response = await fetch(`${baseUrl}/User/Login`, {
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
                    await handleLoginSuccess(responseData);
                } else {
                    alert('Login failed. Please check your credentials.');
                }
            } catch (error) {
                alert('An error occurred during login. Please try again.');
            }
        },
    },
});
