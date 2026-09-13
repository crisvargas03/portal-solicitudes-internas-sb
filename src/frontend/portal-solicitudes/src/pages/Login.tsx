import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Navigate, useNavigate, useSearchParams } from 'react-router';
import logo from '../assets/SUPERINTENDENCIA_DE_BANCOS.png';
import { ErrorApi } from '../lib/apiClient';
import { loginSchema, type LoginFormValues } from '../schemas/loginSchema';
import { iniciarSesion } from '../services/authService';
import { Button } from '../components/ui/Button';
import { FormField } from '../components/ui/FormField';
import { useAuthStore } from '../store/authStore';

export function Login() {
	const navigate = useNavigate();
	const [searchParams] = useSearchParams();
	const user = useAuthStore(state => state.user);
	const guardarSesion = useAuthStore(state => state.iniciarSesion);
	const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

	const {
		register,
		handleSubmit,
		setError,
		formState: { errors, isSubmitting },
	} = useForm<LoginFormValues>({
		resolver: zodResolver(loginSchema),
		mode: 'onTouched',
		defaultValues: { email: '', password: '' },
	});

	// Con sesión ya activa (p. ej. token persistido revalidado), /login no tiene nada que hacer.
	if (user) {
		return <Navigate to='/dashboard' replace />;
	}

	const volverA = searchParams.get('volverA');
	const sesionExpirada = searchParams.get('sesionExpirada') === '1';

	async function onSubmit(valores: LoginFormValues) {
		setErrorGeneral(null);

		try {
			const sesion = await iniciarSesion(valores);
			guardarSesion(sesion);
			navigate(volverA || '/dashboard', { replace: true });
		} catch (error) {
			if (error instanceof ErrorApi) {
				if (error.errores) {
					// 400 de validación: claves PascalCase del backend (Email, Password) → campos del form.
					for (const [campo, mensajes] of Object.entries(
						error.errores,
					)) {
						const nombreCampo =
							campo.toLowerCase() as keyof LoginFormValues;
						if (mensajes[0]) {
							setError(nombreCampo, { message: mensajes[0] });
						}
					}
					return;
				}

				setErrorGeneral(error.message);
				return;
			}

			setErrorGeneral('No se pudo iniciar sesión. Intente de nuevo.');
		}
	}

	return (
		<div className='flex min-h-screen items-center justify-center bg-navy px-4'>
			<div className='w-full max-w-sm rounded-xl bg-white p-8 shadow-sm'>
				<img
					src={logo}
					alt='Superintendencia de Bancos'
					className='mx-auto mb-6 h-14 w-auto'
				/>
				<h1 className='mb-6 text-center text-lg font-semibold text-navy'>
					Portal de Solicitudes Internas
				</h1>

				{sesionExpirada && (
					<p className='mb-4 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger'>
						Su sesión expiró. Vuelva a iniciar sesión.
					</p>
				)}
				{errorGeneral && (
					<p className='mb-4 rounded-md bg-danger-soft px-3 py-2 text-xs text-danger'>
						{errorGeneral}
					</p>
				)}

				<form
					onSubmit={handleSubmit(onSubmit)}
					className='flex flex-col gap-4'
					noValidate>
					<FormField
						etiqueta='Correo institucional'
						error={errors.email?.message}>
						<input
							type='email'
							placeholder='nombre@sb.gob.do'
							{...register('email')}
						/>
					</FormField>

					<FormField
						etiqueta='Contraseña'
						error={errors.password?.message}>
						<input
							type='password'
							placeholder='••••••••'
							{...register('password')}
						/>
					</FormField>

					<Button
						type='submit'
						cargando={isSubmitting}
						className='mt-2'>
						Iniciar sesión
					</Button>
				</form>
			</div>
		</div>
	);
}
