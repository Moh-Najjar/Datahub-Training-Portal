interface LogoProps {
  height?: number;
  className?: string;
}

export function Logo({ height = 32, className }: LogoProps) {
  return (
    <img
      src="/logo.png"
      alt="Datahub Analytics"
      height={height}
      style={{ height, width: "auto" }}
      className={className}
    />
  );
}
