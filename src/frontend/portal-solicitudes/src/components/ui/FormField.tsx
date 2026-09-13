import type { ReactElement } from 'react';
import { cloneElement, useId } from 'react';
import { inputClasses } from './inputClasses';

interface FormFieldProps {
	etiqueta: string;
	error?: string;
	/** El control (input/select/textarea) a envolver; recibe className, aria-invalid y aria-describedby. */
	children: ReactElement<{
		'className'?: string;
		'aria-invalid'?: boolean;
		'aria-describedby'?: string;
	}>;
	className?: string;
}

/**
 * Label + control + mensaje de error inline, consistente en toda la app. El error solo debe
 * pasarse cuando el campo ya fue tocado o se intentó enviar el formulario (mode: 'onTouched'
 * en useForm) — este componente no decide eso, solo renderiza lo que le llega.
 */
export function FormField({
	etiqueta,
	error,
	children,
	className,
}: FormFieldProps) {
	const idError = useId();
	const control = cloneElement(children, {
		'className': `${inputClasses(Boolean(error))} ${children.props.className ?? ''}`,
		'aria-invalid': Boolean(error),
		'aria-describedby': error ? idError : undefined,
	});

	return (
		<label
			className={`flex flex-col gap-1.5 text-sm font-medium text-slate-700 ${className ?? ''}`}>
			{etiqueta}
			{control}
			{error && (
				<span id={idError} className='text-xs text-danger'>
					{error}
				</span>
			)}
		</label>
	);
}
