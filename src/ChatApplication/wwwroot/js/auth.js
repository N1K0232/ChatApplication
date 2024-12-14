function auth(language) {
    Alpine.data("auth", () => ({
        firstName: '',
        lastName: '',
        userName: '',
        email: '',
        password: '',
        errorMessage: '',
        isBusy: false,

        login: async function () {
            this.isBusy = true;

            try {
                const response = await loginAsync(this.email, this.password, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null) {
                    window.localStorage.setItem('access_token', content.accessToken);
                }
            } catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        register: async function () {
            this.isBusy = true;

            try {
                const response = await registerAsync(this.firstName, this.lastName, this.userName, this.email, this.password, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null) {
                    window.location.href = '/';
                }
            } catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        }
    }));
}

async function loginAsync(email, password, language) {
    const request = {
        email: email,
        password: password
    };

    const response = await fetch('/api/auth/login', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function registerAsync(firstName, lastName, userName, email, password, language) {
    const request = {
        firstName: firstName,
        lastName: lastName,
        userName: userName,
        email: email,
        password: password
    };

    const response = await fetch('/api/auth/register', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}